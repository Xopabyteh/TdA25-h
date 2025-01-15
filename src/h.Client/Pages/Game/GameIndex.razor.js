const SYMBOL_X = 0;
const SYMBOL_O = 1;

const X_IMG_PATH = '/img/X/X_cervene.svg';
const O_IMG_PATH = '/img/O/O_modre.svg';
const SYMBOLS_IN_ROW_FOR_WIN = 5;
const ortoAndDiagonalDirections = [
    { x: 0, y: 1 },
    { x: 1, y: 1 },
    { x: 1, y: 0 },
    { x: 1, y: -1 },

    { x: 0, y: -1 },
    { x: -1, y: -1 },
    { x: -1, y: 0 },
    { x: -1, y: 1 }
];

// Properties are all reinitialized when the game is initialized (due to js module system)
let gameField = []; // [y][x], y - row, x - column
let fieldWidth = 15;
let fieldHeight = 15;

let symbolOnMove = SYMBOL_X;
let moveIndex = 0;
let dotnetRef;

export const initializeGame = (
    _gameFieldElementRef = new Element(),
    _dotnetRef = new Object(),
    _fieldWidth = 15,
    _fieldHeight = 15,
    _loadedField = null // Optional, used when editing existing game
) => {
    dotnetRef = _dotnetRef;

    symbolOnMove = SYMBOL_X;
    moveIndex = 0;

    fieldWidth = _fieldWidth;
    fieldHeight = _fieldHeight;

    // Prepare field
    gameField = [];
    for (let y = 0; y < fieldHeight; y++) {
        gameField.push([]);
        for (let x = 0; x < fieldWidth; x++) {
            gameField[y].push();
        }
    }

    // Add onclick event to each cell
    const cells = _gameFieldElementRef.querySelectorAll('.cell');
    cells.forEach(cell => {
        const x = Number.parseInt(cell.getAttribute('data-x'));
        const y = Number.parseInt(cell.getAttribute('data-y'));

        cell.addEventListener('click',
            () => handleCellClick(cell, x, y)
        );
    });
}

const handleCellClick = (
    cellElement = new Element(),
    x = 0,
    y = 0
) => {
    // Try Set symbol
    const couldSetSymbol = trySetSymbol(x, y, symbolOnMove, cellElement);
    if (!couldSetSymbol) {
        return;
    }

    // Increase move index
    moveIndex++;

    // Check for win
    if(checkWinFromNewPos(x, y)) {
        dotnetRef.invokeMethodAsync('SetWinner', symbolOnMove == SYMBOL_X);
        return;
    };

    // Change symbol on turn
    symbolOnMove = symbolOnMove === SYMBOL_X
        ? SYMBOL_O
        : SYMBOL_X;

    // Sync with .NET
    dotnetRef.invokeMethodAsync(
        'OnMoved',
        symbolOnMove == SYMBOL_X,
        moveIndex,
        getTurn()
    );
}

/**
 * A turn is when both players have made a move
 */
const getTurn = () => {
    return moveIndex % 2;
}


/**
 * Appends img to cell and sets field 2D array value.
 * Returns false if the cell is already occupied.
 * @param {any} x
 * @param {any} y
 * @param {any} symbol
 * @param {any} cell
 * @returns {boolean}
 */
const trySetSymbol = (
    x = 0,
    y = 0,
    symbol = SYMBOL_X,
    cell = new Element()
) => {
    if (gameField[y][x] !== undefined) {
        return false;
    }

    gameField[y][x] = symbol;

    const img = document.createElement('img');
    img.src = symbol === SYMBOL_X ? X_IMG_PATH : O_IMG_PATH;
    img.alt = symbol === SYMBOL_X ? 'X' : 'O';

    cell.innerHTML = '';
    cell.appendChild(img);
    cell.classList.remove('empty');

    return true;
}

const checkWinFromNewPos = (
    x = 0,
    y = 0
) => {
    for (let i = 0; i < ortoAndDiagonalDirections.length; i++) {
        const direction = ortoAndDiagonalDirections[i];
        if (isWinningSequence(x, y, direction)) {
            return true;
        }
    }

    return false;
}

/**
 * Checks if there's a winning sequence starting from (startX, startY) in a given direction
 * @param {number} startX - Starting x coordinate
 * @param {number} startY - Starting y coordinate
 * @param {Object} direction - Direction to check { x: number, y: number }
 * @returns {boolean}
 */
const isWinningSequence = (
    startX = 0,
    startY = 0,
    direction
) => {
    let count = 0;
    let x = startX;
    let y = startY;

    while (
        x >= 0
        && x < fieldWidth
        && y >= 0
        && y < fieldHeight) {
        if (gameField[y][x] !== symbolOnMove) {
            break;
        }

        count++;
        if (count === SYMBOLS_IN_ROW_FOR_WIN) {
            return true;
        }

        x += direction.x;
        y += direction.y;
    }

    return false;
}