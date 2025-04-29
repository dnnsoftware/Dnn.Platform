import { Component, Host, h, Prop, Element } from "@stencil/core";

@Component({
  tag: "dnn-rm-progress-bar",
  styleUrl: "dnn-rm-progress-bar.scss",
  shadow: true,
})
export class DnnRmProgressBar {
  /** Defines the current progress value. */
  @Prop() value: number = 0;

  /** Defines the max progress value. */
  @Prop() max: number = 100;

  componentDidLoad() {
    this.adjustProgress();
  }

  componentDidUpdate() {
    this.adjustProgress();
  }

  private adjustProgress() {
    this.el.style.setProperty("--current-value", this.value.toString());
    this.el.style.setProperty("--max-value", this.max.toString());
  }

  @Element() el: HTMLDnnRmProgressBarElement;

  render() {
    return (
      <Host>
        <div class="progress-container">
          <div class="progress-bar"> </div>
          <div class="progress-bar-remainder" />
        </div>
      </Host>
    );
  }
}
