import { Component, Host, h } from '@stencil/core';
import { Ip } from './ip-safelist.model';

@Component({
  tag: 'ip-safelist',
  styleUrl: 'ip-safelist.scss',
  shadow: true,
})
export class IpSafelist {

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
                <h3 class="panel-title">New IP Safelist Entry</h3>
              </div>
              <div class="panel-body">
                <div class="form-horizontal">
                  <div class="form-group">
                    <dnn-input type="text" label="Name" helpText="Enter IP Address name" required></dnn-input>
                    <dnn-input type="text" label="IP Address" helpText="Enter IP Address" required></dnn-input>
                    <dnn-button appearance="primary" onClick={() => this.createIp(this.newIp)}>Add</dnn-button>
                  </div>
                </div>
  
                <div class="clearfix"></div>
              </div>
            </div>
          </div>
  
          <div class="col">
            <div class="panel">
              <div class="panel-heading">
                <h3 class="panel-title">IP Safelist Entries</h3>
              </div>
              <div class="panel-body">
                <table class="table">
                  <thead>
                    <tr>
                      <th>Name</th>
                      <th>IP Address</th>
                      <th>Action</th>
                    </tr>
                  </thead>
                  <tbody>
                    {this.ipSafelist.map((ip) => (
                      <tr>
                        <td>{ip.name}</td>
                        <td>{ip.ipAddress}</td>
                        <td><dnn-button appearance="danger" size="small" onClick={() => this.deleteIp(ip)}>Delete</dnn-button></td>
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
                <h3 class="panel-title">IP Safelist Configuration</h3>
              </div>
              <div class="panel-body">
                <div class="form-horizontal">
                  <div class="form-group">
                    <label>
                      <dnn-toggle name="enableIpSafelist" checked={this.enableIpSafelist}></dnn-toggle>
                      Enable IP Safelist
                    </label> 
                    <dnn-button appearance="primary" onClick={() => this.saveIpSafelistConfiguration(this.enableIpSafelist)}>Save</dnn-button>
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
