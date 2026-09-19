window.blogWriterDialog = {
    previousFocus: null,
    show(dialog) {
        this.previousFocus = document.activeElement;
        if (dialog && !dialog.open) {
            dialog.showModal();
        }
    },
    close(dialog) {
        if (dialog?.open) {
            dialog.close();
        }
        if (this.previousFocus instanceof HTMLElement) {
            this.previousFocus.focus();
        }
        this.previousFocus = null;
    }
};
