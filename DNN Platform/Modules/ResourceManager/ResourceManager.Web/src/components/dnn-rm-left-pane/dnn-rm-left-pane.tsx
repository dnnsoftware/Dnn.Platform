import { Component, Host, h } from '@stencil/core';

@Component({
  tag: 'dnn-rm-left-pane',
  styleUrl: 'dnn-rm-left-pane.css',
  shadow: true,
})
export class DnnRmLeftPane {

  render() {
    return (
      <Host>
        <p>I am the left pane</p>
      </Host>
    );
  }

}
