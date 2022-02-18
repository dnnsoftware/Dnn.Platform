import { Component, Host, h } from '@stencil/core';

@Component({
  tag: 'dnn-rm-status-bar',
  styleUrl: 'dnn-rm-status-bar.css',
  shadow: true,
})
export class DnnRmStatusBar {

  render() {
    return (
      <Host>
        <slot></slot>
      </Host>
    );
  }

}
