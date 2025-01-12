import { Component, Host, h, Prop } from '@stencil/core';
import { LocalizationClient } from '../../clients/localization-client';
import state from "../../stores/store";

@Component({
  tag: 'dnn-bulk-install',
  styleUrl: 'dnn-bulk-install.scss',
  shadow: false,
})
export class DnnBulkInstall {
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
                <bulk-install-install></bulk-install-install>
              </div>
            </dnn-tab>
            <dnn-tab tabTitle={state.resx.Events}>
              <div class="tab-content">
                <bulk-install-logs></bulk-install-logs>
              </div>
            </dnn-tab>
            <dnn-tab tabTitle={state.resx.ApiUsers}>
              <div class="tab-content">
                <bulk-install-api-users></bulk-install-api-users>
              </div>
            </dnn-tab>
            <dnn-tab tabTitle={state.resx.IPSafeList}>
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
