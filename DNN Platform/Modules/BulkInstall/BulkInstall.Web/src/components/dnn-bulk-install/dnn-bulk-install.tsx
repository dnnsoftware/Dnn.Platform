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
            <dnn-tabs>
              <dnn-tab tabTitle="Install">
                <div class="tab-content">Install UI goes here.</div>
              </dnn-tab>
              <dnn-tab tabTitle="Event Log">
                <div class="tab-content">Event Log UI goes here.</div>
              </dnn-tab>
              <dnn-tab tabTitle="API Users">
                <div class="tab-content">API Users UI goes here.</div>
              </dnn-tab>
              <dnn-tab tabTitle="IP Safelist">
                <div class="tab-content">IP Safelist UI goes here.</div>
              </dnn-tab>
            </dnn-tabs>
          </div>
        </div>
      </Host>
    );
  }
}
