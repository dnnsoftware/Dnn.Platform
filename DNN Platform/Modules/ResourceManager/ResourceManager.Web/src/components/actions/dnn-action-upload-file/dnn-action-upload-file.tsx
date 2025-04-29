import { Component, Host, h, Event, EventEmitter, Prop } from "@stencil/core";
import state from "../../../store/store";

@Component({
  tag: "dnn-action-upload-file",
  styleUrl: "../dnn-action.scss",
  shadow: true,
})
export class DnnActionUploadFile {
  @Prop() parentFolderId: number;

  /**
   * Fires when there is a possibility that some folders have changed.
   * Can be used to force parts of the UI to refresh.
   */
  @Event() dnnRmFoldersChanged: EventEmitter<void>;

  private handleClick(): void {
    this.showModal();
  }

  private showModal() {
    const modal = document.createElement("dnn-modal");
    modal.backdropDismiss = true;
    modal.showCloseButton = true;
    const container = document.createElement("div");
    container.style.overflowY = "auto";
    container.style.maxHeight = "70vh";
    const editor = document.createElement("dnn-rm-upload-file");
    container.appendChild(editor);
    modal.appendChild(container);
    document.body.appendChild(modal);
    modal.show();
    modal.addEventListener("dismissed", () => {
      this.dnnRmFoldersChanged.emit();
    });
  }

  render() {
    return (
      <Host>
        <button onClick={() => this.handleClick()}>
          <svg xmlns="http://www.w3.org/2000/svg" height="24" width="24">
            <path d="M6 20q-.825 0-1.412-.587Q4 18.825 4 18v-3h2v3h12v-3h2v3q0 .825-.587 1.413Q18.825 20 18 20Zm5-4V7.85l-2.6 2.6L7 9l5-5 5 5-1.4 1.45-2.6-2.6V16Z" />
          </svg>
          <span>{state.localization.Upload}</span>
        </button>
      </Host>
    );
  }
}
