const X_IMG_PATH = '/IMG/X/X_cervene.svg';
const O_IMG_PATH = '/IMG/O/O_modre.svg';

// 'X' or 'O' or '' for eraser
const PENCIL_X = 'X';
const PENCIL_O = 'O';
const PENCIL_ERASER = '';

let selectedPencil = null;
let gameField = []; // [y][x], y - row, x - column
let gameFieldElementRef;

let editHistory = [];
let historyI = 0;

// Needed, because js variables persists after leaving page
const reset = () => {
    gameField = [];
    gameFieldElementRef = null;
    editHistory = [];
    historyI = 0;
    selectedPencil = null;
}

export const initializeEditor = (
    _gameFieldElementRef = new Element(),
    fieldWidth = 15,
    fieldHeight = 15,
    loadedField = null // Optional, used when editing existing game
) => {
    reset();

    gameFieldElementRef = _gameFieldElementRef;

    // Prepare field
    for (let y = 0; y < fieldHeight; y++) {
        gameField.push([]);
        for (let x = 0; x < fieldWidth; x++) {
            gameField[y].push(PENCIL_ERASER);
        }
    }

    if (loadedField != null) {
        loadField(loadedField);
    }
    
    appendCurrentStateToEditHistory();

    // Add onclick event to each cell
    const cells = gameFieldElementRef.querySelectorAll('.cell');
    cells.forEach(cell => {
        cell.addEventListener('click', () => handleCellClick(cell));
    });
}

const handleCellClick = (
    cell = new Element()
) => {
    if (selectedPencil === null)
        return; // No pencil selected

    const x = cell.getAttribute('data-x');
    const y = cell.getAttribute('data-y');

    // Set field value
    const shouldAppendToHistory = gameField[y][x] !== selectedPencil;
    gameField[y][x] = selectedPencil;

    // Add/remove image
    let img = cell.querySelector('img');

    if (selectedPencil === PENCIL_ERASER) {
        // Eraser
        if(img) {
            cell.removeChild(img);
            if (shouldAppendToHistory) {
                appendCurrentStateToEditHistory();
            }
        }

        return;
    }

    // -> Draw
    const newImgSrc = selectedPencil === PENCIL_X
        ? X_IMG_PATH
        : O_IMG_PATH;

    // Append to history if value changed
    if (shouldAppendToHistory) {
        appendCurrentStateToEditHistory();
    }

    if (img) {
        // Only update src
        img.src = newImgSrc;
        return;
    }

    // -> Create new img element
    img = document.createElement('img');
    img.src = newImgSrc;
    cell.appendChild(img);
}

export const selectXPencil = () => {
    if (gameFieldElementRef == null)
        return;

    selectedPencil = PENCIL_X;

    gameFieldElementRef.setAttribute('data-cell-hover-symbol', PENCIL_X);
}
 
export const selectOPencil = () => {
    if (gameFieldElementRef == null)
        return;

    selectedPencil = PENCIL_O;

    gameFieldElementRef.setAttribute('data-cell-hover-symbol', PENCIL_O);
}

export const selectEraser = () => {
    if (gameFieldElementRef == null)
        return;

    selectedPencil = PENCIL_ERASER;

    gameFieldElementRef.setAttribute('data-cell-hover-symbol', PENCIL_ERASER);
}

export const clearCanvas = () => {
    const isClearAlready = gameField.every(row => row.every(cell => cell === ''));
    if(isClearAlready)
        return;

    // Clear game field
    for (let y = 0; y < gameField.length; y++) {
        for (let x = 0; x < gameField[y].length; x++) {
            gameField[y][x] = '';
        }
    }

    // Remove all cells
    const cells = gameFieldElementRef.querySelectorAll('.cell');
    cells.forEach(cell => {
        const img = cell.querySelector('img');
        if (img) {
            cell.removeChild(img);
        }
    });

    appendCurrentStateToEditHistory();
}

const loadField = (
    newGameField, // [y][x], y - row, x - column
) => {
    if (gameFieldElementRef == null)
        return; // lets hope this wont happen lol

    // Set game field values
    for (let y = 0; y < gameField.length; y++) {
        for (let x = 0; x < gameField[y].length; x++) {
            gameField[y][x] = newGameField[y][x];
        }
    }

    const cells = gameFieldElementRef.querySelectorAll('.cell');

    // update images
    cells.forEach(cell => {
        const x = cell.getAttribute('data-x');
        const y = cell.getAttribute('data-y');

        const pencil = gameField[y][x];
        let img = cell.querySelector('img');

        if (pencil === PENCIL_ERASER) {
            // Try remove img
            if (img) {
                cell.removeChild(img);
            }

            return;
        }

        const imgSrc = pencil === PENCIL_X
            ? X_IMG_PATH
            : O_IMG_PATH;

        // Draw X or O
        if (img == null) {
            // Create new img element
            img = document.createElement('img');
            img.src = imgSrc;
            cell.appendChild(img);
            return;
        }

        // -> Only update src
        img.src = imgSrc;
    });
}

export const historyBack = () => {
    if(historyI <= 0)
        return;

    historyI--;

    loadField(editHistory[historyI]);
}

export const historyForth = () => {
    if(historyI >= editHistory.length - 1)
        return;

    historyI++;

    loadField(editHistory[historyI]);
}

const appendCurrentStateToEditHistory = () => {
    if(historyI < editHistory.length - 1) {
        editHistory.splice(historyI + 1);
    };

    const gameFieldDeepCopy = [];
    for (let y = 0; y < gameField.length; y++) {
        gameFieldDeepCopy.push([...gameField[y]]);
    }

    editHistory.push(gameFieldDeepCopy);
    historyI = editHistory.length - 1;
}

export const getGameField = () => {
    return gameField;
}