import { Component, Host, h, Prop } from '@stencil/core';
import { LocalizationClient } from '../../clients/localization-client';
import state from '../../stores/store';

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
    state.moduleId = this.moduleId;
    try {
      state.resx = await this.localizationClient.getResources();
    } catch (error) {
      console.error(error);
    }
  }

  render() {
    return (
      <Host>
        <div class="container">
          <dnn-tabs>
            <dnn-tab tabTitle={state.resx.Install}>
              <div class="tab-content">
                <dnn-bi-install></dnn-bi-install>
              </div>
            </dnn-tab>
            <dnn-tab tabTitle={state.resx.Events}>
              <div class="tab-content">
                <dnn-bi-logs></dnn-bi-logs>
              </div>
            </dnn-tab>
            <dnn-tab tabTitle={state.resx.ApiUsers}>
              <div class="tab-content">
                <dnn-bi-api-users></dnn-bi-api-users>
              </div>
            </dnn-tab>
            <dnn-tab tabTitle={state.resx.IPSafeList}>
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
