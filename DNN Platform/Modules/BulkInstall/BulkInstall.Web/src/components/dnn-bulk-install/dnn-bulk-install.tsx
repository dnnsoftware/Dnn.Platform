import { Component, Host, h, Prop } from '@stencil/core';
import store from '../../stores/store';
import { LocalizationClient } from '../../clients/localization-client';

@Component({
  tag: 'dnn-bulk-install',
  styleUrl: 'dnn-bulk-install.scss',
  shadow: false,
})
export class DnnBulkInstall {
  /** The ID of the module. */
  @Prop() moduleId!: number;

  private localizationClient: LocalizationClient;

  constructor() {
    this.localizationClient = new LocalizationClient(this.moduleId);
  }

  async componentWillLoad() {
    store.moduleId = this.moduleId;
    try {
      store.resx = await this.localizationClient.getResources();
    } catch (error) {
      console.error(error);
    }
  }

  render() {
    return (
      <Host>
        <div class="container">
          <dnn-tabs>
            <dnn-tab tabTitle={store.resx.Install}>
              <div class="tab-content">
                <dnn-bi-install></dnn-bi-install>
              </div>
            </dnn-tab>
            <dnn-tab tabTitle={store.resx.Events}>
              <div class="tab-content">
                <dnn-bi-logs></dnn-bi-logs>
              </div>
            </dnn-tab>
            <dnn-tab tabTitle={store.resx.ApiUsers}>
              <div class="tab-content">
                <dnn-bi-api-users></dnn-bi-api-users>
              </div>
            </dnn-tab>
            <dnn-tab tabTitle={store.resx.IPSafeList}>
              <div class="tab-content">
                <dnn-bi-ip-safelist></dnn-bi-ip-safelist>
              </div>
            </dnn-tab>
          </dnn-tabs>
        </div>
      </Host>
    );
  }
}
