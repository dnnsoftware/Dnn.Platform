import { Component, Host, h } from '@stencil/core';

@Component({
  tag: 'dnn-rm-top-bar',
  styleUrl: 'dnn-rm-top-bar.css',
  shadow: true,
})
export class DnnRmTopBar {

  render() {
    return (
      <Host>
        <p>I am the top bar</p>
      </Host>
    );
  }

}
