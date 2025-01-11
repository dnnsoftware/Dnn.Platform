import { Component, Host, h } from '@stencil/core';
import { Ip } from './bulk-install-ip-safelist.model';
import state from "../../../stores/store";

@Component({
  tag: 'bulk-install-ip-safelist',
  styleUrl: 'bulk-install-ip-safelist.scss',
  shadow: true,
})
export class BulkInstallIpSafelist {

  private ipSafelist: Ip[] = [];
  private newIp: Ip = {
    name: '',
    ipAddress: '',
  }
  private enableIpSafelist: boolean = false;

  private createIp(_newIp: Ip): (event: MouseEvent) => void {
    alert('Method not implemented.');
    return;
  }

  private deleteIp(_ip: Ip): (event: MouseEvent) => void {
    alert('Method not implemented.');
    return;
  }

  private saveIpSafelistConfiguration(_enableIpSafelist: boolean): (event: MouseEvent) => void {
    alert('Method not implemented.');
    return;
  }

  render() {
    return (
      <Host>
        <div class="row">
          <div class="col">
            <div class="panel">
              <div class="panel-heading">
                <h3 class="panel-title">{state.resx.NewIpSafelistEntry}</h3>
              </div>
              <div class="panel-body">
                <div class="form-horizontal">
                  <div class="form-group">
                    <dnn-input
                      type="text"
                      label={state.resx.IPSafeListItemNameText}
                      helpText={state.resx.IPSafeListItemNameHelp}
                      required
                    />
                    <dnn-input
                      type="text"
                      label={state.resx.IPSafeListItemIpAddressText}
                      helpText={state.resx.IPSafeListItemIpAddressHelp}
                      required
                    />
                    <dnn-button
                      onClick={() => this.createIp(this.newIp)}
                    >
                      {state.resx.Add}
                    </dnn-button>
                  </div>
                </div>
  
                <div class="clearfix"></div>
              </div>
            </div>
          </div>
          <div class="col">
            <div class="panel">
              <div class="panel-heading">
                <h3 class="panel-title">{state.resx.IPSafeListEntries}</h3>
              </div>
              <div class="panel-body">
                <table class="table">
                  <thead>
                    <tr>
                      <th>{state.resx.Name}</th>
                      <th>{state.resx.IPAddress}</th>
                      <th>{state.resx.Action}</th>
                    </tr>
                  </thead>
                  <tbody>
                    {this.ipSafelist.map((ip) => (
                      <tr>
                        <td>{ip.name}</td>
                        <td>{ip.ipAddress}</td>
                        <td>
                          <dnn-button
                            appearance="danger"
                            size="small"
                            onClick={() => this.deleteIp(ip)}
                          >
                            {state.resx.Delete}
                          </dnn-button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          </div>
          <div class="col">
            <div class="panel">
              <div class="panel-heading">
                <h3 class="panel-title">{state.resx.IPSafeListConfiguration}</h3>
              </div>
              <div class="panel-body">
                <div class="form-horizontal">
                  <div class="form-group">
                    <label>
                      <dnn-toggle name="enableIpSafelist" checked={this.enableIpSafelist}></dnn-toggle>
                      {state.resx.EnableIpSafeList}
                    </label> 
                    <dnn-button
                      onClick={() => this.saveIpSafelistConfiguration(this.enableIpSafelist)}
                    >
                      {state.resx.Save}
                    </dnn-button>
                  </div>
                </div>
                <div class="clearfix"></div>
              </div>
            </div>
          </div>
        </div>
      </Host>
    );
  }
}
