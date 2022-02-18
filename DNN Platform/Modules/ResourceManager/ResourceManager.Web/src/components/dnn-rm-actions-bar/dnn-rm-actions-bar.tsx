import { Component, Host, h } from '@stencil/core';

@Component({
  tag: 'dnn-rm-actions-bar',
  styleUrl: 'dnn-rm-actions-bar.css',
  shadow: true,
})
export class DnnRmActionsBar {

  render() {
    return (
      <Host>
        <slot></slot>
      </Host>
    );
  }

}
