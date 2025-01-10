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
        <div class="container">
          <dnn-tabs>
            <dnn-tab tabTitle="Install">
              <div class="tab-content">Install UI goes here.</div>
            </dnn-tab>
            <dnn-tab tabTitle="Event Log">
              <bulk-install-logs></bulk-install-logs>
            </dnn-tab>
            <dnn-tab tabTitle="API Users">
              <div class="tab-content">
                <bulk-install-api-users></bulk-install-api-users>
              </div>
            </dnn-tab>
            <dnn-tab tabTitle="IP Safelist">
              <div class="tab-content">
                <bulk-install-ip-safelist></bulk-install-ip-safelist>
              </div>
            </dnn-tab>
          </dnn-tabs>
        </div>
      </Host>
    );
  }
}
