import { Component, Host, h } from '@stencil/core';

@Component({
  tag: 'dnn-rm-left-pane',
  styleUrl: 'dnn-rm-left-pane.scss',
  shadow: true,
})
export class DnnRmLeftPane {

  render() {
    return (
      <Host>
        <ul>
          <li>Folders</li>
          <li>will</li>
          <li>go</li>
          <li>over</li>
          <li>here</li>
        </ul>
      </Host>
    );
  }

}
