import { Component, Host, h } from '@stencil/core';

@Component({
  tag: 'dnn-bulk-install',
  styleUrl: 'dnn-bulk-install.scss',
  shadow: false,
})
export class DnnBulkInstall {
  render() {
    return (
      <Host>
        <div class="aperture-container">
          <div class="aperture-d-flex aperture-flex-column aperture-flex-lg-row aperture-my-4">
            Hello, World! I'm a placeholder for the new Bulk Install user interface.
          </div>
        </div>
      </Host>
    );
  }
}
