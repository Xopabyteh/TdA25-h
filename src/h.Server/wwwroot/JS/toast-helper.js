var ToastHelper = ToastHelper || {};

ToastHelper.error = (
	messaage = ""
) => {
	const notyf = new Notyf({ position: { x: 'center', y: 'bottom' } });
	notyf.error(messaage);
}

ToastHelper.success = (
	messaage = ""
) => {
	const notyf = new Notyf({ position: { x: 'center', y: 'bottom' } });
	notyf.success(messaage);
}