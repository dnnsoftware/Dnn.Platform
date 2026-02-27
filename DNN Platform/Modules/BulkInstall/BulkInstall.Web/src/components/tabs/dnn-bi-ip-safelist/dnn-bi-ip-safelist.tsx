import { Component, Host, h, State } from '@stencil/core';
import store from '../../../stores/store';
import { IpSafelistClient } from '../../../clients/ip-safelist-client';
import { Ip } from './dnn-bi-ip-safelist.model';

@Component({
  tag: 'dnn-bi-ip-safelist',
  styleUrl: 'dnn-bi-ip-safelist.scss',
  shadow: true,
})
export class DnnBiIpSafelist {
  @State() private ipSafelist: Ip[] = [];
  @State() private newIp: Ip = {
    id: -1,
    name: '',
    ipAddress: '',
  };
  @State() private enableIpSafelist: boolean = false;

  private ipSafelistClient: IpSafelistClient;

  constructor() {
    this.ipSafelistClient = new IpSafelistClient(store.moduleId);
  }

  async componentWillLoad() {
    try {
      this.ipSafelist = await this.ipSafelistClient.getAll();
      this.enableIpSafelist = await this.ipSafelistClient.getIpSafelistConfiguration();
    } catch (error) {
      console.error(error);
    }
  }

  private async createIp(_newIp: Ip): Promise<void> {
    const createdIp = await this.ipSafelistClient.create(_newIp.name, _newIp.ipAddress);
    this.ipSafelist = [...this.ipSafelist, createdIp];
    this.newIp = {
      id: -1,
      name: '',
      ipAddress: '',
    };
  }

  private async deleteIp(_ip: Ip): Promise<void> {
    await this.ipSafelistClient.delete(_ip.id);
    this.ipSafelist = await this.ipSafelistClient.getAll();
  }

  private async saveIpSafelistConfiguration(_enableIpSafelist: boolean): Promise<void> {
    await this.ipSafelistClient.saveIpSafelistConfiguration(_enableIpSafelist);
  }

  render() {
    return (
      <Host>
        <div class="row">
          <div class="col">
            <div class="panel">
              <div class="panel-heading">
                <h3 class="panel-title">{store.resx.NewIpSafelistEntry}</h3>
              </div>
              <div class="panel-body">
                <div class="form-horizontal">
                  <div class="form-group">
                    <dnn-input
                      type="text"
                      label={store.resx.IPSafeListItemNameText}
                      helpText={store.resx.IPSafeListItemNameHelp}
                      required
                      value={this.newIp.name}
                      onValueInput={e => (this.newIp = { ...this.newIp, name: e.detail as string })}
                    />
                    <dnn-input
                      type="text"
                      label={store.resx.IPSafeListItemIpAddressText}
                      helpText={store.resx.IPSafeListItemIpAddressHelp}
                      required
                      value={this.newIp.ipAddress}
                      onValueInput={e => (this.newIp = { ...this.newIp, ipAddress: e.detail as string })}
                    />
                    <dnn-button
                      onClick={() => {
                        this.createIp(this.newIp).catch(console.error);
                        return;
                      }}
                    >
                      {store.resx.Add}
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
                <h3 class="panel-title">{store.resx.IPSafeListEntries}</h3>
              </div>
              <div class="panel-body">
                <table class="table">
                  <thead>
                    <tr>
                      <th>{store.resx.Name}</th>
                      <th>{store.resx.IPAddress}</th>
                      <th>{store.resx.Action}</th>
                    </tr>
                  </thead>
                  <tbody>
                    {this.ipSafelist.map(ip => (
                      <tr>
                        <td>{ip.name}</td>
                        <td>{ip.ipAddress}</td>
                        <td>
                          <dnn-button
                            appearance="danger"
                            size="small"
                            onClick={() => {
                              this.deleteIp(ip).catch(console.error);
                              return;
                            }}
                          >
                            {store.resx.Delete}
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
                <h3 class="panel-title">{store.resx.IPSafeListConfiguration}</h3>
              </div>
              <div class="panel-body">
                <div class="form-horizontal">
                  <div class="form-group">
                    <label>
                      <dnn-toggle name="enableIpSafelist" checked={this.enableIpSafelist} onCheckChanged={e => (this.enableIpSafelist = e.detail.checked)} />
                      {store.resx.EnableIpSafeList}
                    </label>
                    <dnn-button
                      onClick={() => {
                        this.saveIpSafelistConfiguration(this.enableIpSafelist).catch(console.error);
                        return;
                      }}
                    >
                      {store.resx.Save}
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
