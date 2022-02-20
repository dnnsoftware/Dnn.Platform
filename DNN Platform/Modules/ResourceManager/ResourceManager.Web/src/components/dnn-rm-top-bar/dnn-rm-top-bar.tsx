import { Component, Host, h, } from '@stencil/core';
@Component({
  tag: 'dnn-rm-top-bar',
  styleUrl: 'dnn-rm-top-bar.scss',
  shadow: true,
})
export class DnnRmTopBar {

  render() {
    return (
      <Host>
        <dnn-searchbox></dnn-searchbox>
      </Host>
    );
  }
}
