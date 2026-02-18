import { Component, Host, h, State } from '@stencil/core';
import store from '../../../stores/store'
import {InstallClient} from "../../../clients/install-client";

@Component({
  tag: 'bulk-install-install',
  styleUrl: 'bulk-install-install.scss',
  shadow: true,
})
export class BulkInstallInstall {
  @State() private selectedFiles: File[] = [];

  private sessionGuid: string;
  private installClient: InstallClient;

  constructor(){
    this.installClient = new InstallClient(store.moduleId);
  }

  async installPackages() {
    this.sessionGuid = await this.installClient.create();
    await this.installClient.addPackages(this.sessionGuid, this.selectedFiles);
  }

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
                <dnn-dropzone
                  allowed-extensions={['zip']}
                  onFilesSelected={e => this.selectedFiles = [...this.selectedFiles, ...e.detail] }
                  resx={
                    {
                      dragAndDropFile: store.resx["DropZone.DragAndDropFile"],
                      or: store.resx["DropZone.Or"],
                      uploadFile: store.resx["DropZone.UploadFile"],
                    }
                }></dnn-dropzone>
                {this.selectedFiles.length > 0 && <ul>
                  {this.selectedFiles.map(file => <li key={file.name}>{file.name}</li>)}
                </ul>}
                <div class="form-group">
                  <dnn-button disabled={ this.selectedFiles.length < 1 } onClick={_ => this.installPackages() }>Install</dnn-button>
                  <dnn-button appearance="tertiary" reversed onClick={_ => this.selectedFiles = [] }>Reset</dnn-button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </Host>
    );
  }
}
