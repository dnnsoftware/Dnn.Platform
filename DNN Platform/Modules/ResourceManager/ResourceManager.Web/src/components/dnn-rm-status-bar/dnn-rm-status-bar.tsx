import { Component, Host, h } from '@stencil/core';

@Component({
  tag: 'dnn-rm-status-bar',
  styleUrl: 'dnn-rm-status-bar.scss',
  shadow: true,
})
export class DnnRmStatusBar {

  render() {
    return (
      <Host>
        I am the status bar
      </Host>
    );
  }

}
