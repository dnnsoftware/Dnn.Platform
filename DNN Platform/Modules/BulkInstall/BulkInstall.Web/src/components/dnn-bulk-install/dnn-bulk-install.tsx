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
              <div class="tab-content">Install UI goes here.</div>
            </dnn-tab>
            <dnn-tab tabTitle={state.resx.Events}>
              <div class="tab-content">Event Log UI goes here.</div>
            </dnn-tab>
            <dnn-tab tabTitle={state.resx.ApiUsers}>
              <div class="tab-content">
                <api-users></api-users>
              </div>
            </dnn-tab>
            <dnn-tab tabTitle={state.resx.IPSafeList}>
              <div class="tab-content">
                <ip-safelist></ip-safelist>
              </div>
            </dnn-tab>
          </dnn-tabs>
        </div>
      </Host>
    );
  }
}
