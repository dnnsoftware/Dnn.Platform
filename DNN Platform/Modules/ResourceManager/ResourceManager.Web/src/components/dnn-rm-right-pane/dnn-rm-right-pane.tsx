import { Component, Host, h } from '@stencil/core';

@Component({
  tag: 'dnn-rm-right-pane',
  styleUrl: 'dnn-rm-right-pane.css',
  shadow: true,
})
export class DnnRmRightPane {

  render() {
    return (
      <Host>
        <p>I am the right pane</p>
      </Host>
    );
  }

}
