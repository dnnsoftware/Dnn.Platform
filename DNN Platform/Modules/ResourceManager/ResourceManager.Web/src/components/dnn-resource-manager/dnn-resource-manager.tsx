import { Component, Host, h } from '@stencil/core';

@Component({
  tag: 'dnn-resource-manager',
  styleUrl: 'dnn-resource-manager.scss',
  shadow: true,
})
export class DnnResourceManager {

  render() {
    return (
      <Host>
        <slot></slot>
      </Host>
    );
  }

}
