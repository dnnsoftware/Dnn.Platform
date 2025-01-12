import { Component, Host, h } from '@stencil/core';

@Component({
  tag: 'bulk-install-install',
  styleUrl: 'bulk-install-install.scss',
  shadow: true,
})
export class BulkInstallInstall {

  render() {
    return (
      <Host>
        <div class="row">
          <div class="col">
            <div class="panel">
              <div class="panel-heading">
                <h3 class="panel-title">Upload Install Package(s)</h3>
              </div>
              <div class="panel-body">
                <dnn-dropzone allowed-extensions="zip"></dnn-dropzone>
                <div class="form-group">
                  <dnn-button>Install</dnn-button>
                  <dnn-button appearance="tertiary" reversed>Reset</dnn-button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </Host>
    );
  }
}
