import { Component, Host, h, State } from '@stencil/core';
import { User } from './dnn-bi-api-users.model';
import state from '../../../stores/store';
import { ApiUserClient } from '../../../clients/api-user-client';

interface NewUser {
  name: string;
  bypassIPWhitelist: boolean;
  expiresOn: Date;
}

function addYear(date: Date): Date {
  return new Date(date.getTime() + 365 * 24 * 60 * 60 * 1000);
}

function toISODate(date: Date): string {
  return date.toISOString().substring(0, 10);
}

@Component({
  tag: 'dnn-bi-api-users',
  styleUrl: 'dnn-bi-api-users.scss',
  shadow: true,
})
export class DnnBiApiUsers {
  @State() private users: User[] = [];
  @State() private enabled: boolean | null = null;
  @State() private newUser: NewUser = {
    name: '',
    bypassIPWhitelist: false,
    expiresOn: addYear(new Date()),
  };

  private newUserModal: HTMLDnnModalElement;

  private apiUserClient: ApiUserClient;

  constructor() {
    this.apiUserClient = new ApiUserClient(state.moduleId);
  }

  async componentWillLoad() {
    try {
      const { users, enabled } = await this.apiUserClient.getAll();
      this.users = users;
      this.enabled = enabled;
    } catch (error) {
      console.error(error);
    }
  }

  private async createUser(_newUser: NewUser): Promise<void> {
    const createdUser = await this.apiUserClient.create(_newUser.name, _newUser.bypassIPWhitelist, _newUser.expiresOn);
    this.users = [...this.users, createdUser];
    this.newUser = {
      name: '',
      bypassIPWhitelist: false,
      expiresOn: addYear(new Date()),
    };
    await this.newUserModal.hide();
  }

  private async deleteUser(_user: User): Promise<void> {
    await this.apiUserClient.delete(_user.id);
    const { users } = await this.apiUserClient.getAll();
    this.users = users;
  }

  render() {
    return (
      <Host>
        {this.enabled === false && (
          <div class="row">
            <div class="col">
              <h3 class="danger">{state.resx.ApiAuthDisabled}</h3>
            </div>
          </div>
        )}
        <div class="row">
          <div class="col">
            <div class="button-row">
              <dnn-button
                size="small"
                disabled={this.enabled === false}
                onClick={() => {
                  this.newUserModal.show().catch(console.error);
                  return;
                }}
              >
                {state.resx.NewApiUser}
              </dnn-button>
            </div>
            <div class="panel">
              <div class="panel-heading">
                <h3 class="panel-title">{state.resx.ApiUsers}</h3>
              </div>
              <div class="panel-body">
                <table class="table">
                  <thead>
                    <tr>
                      <th>{state.resx.Name}</th>
                      <th>{state.resx.ApiKey}</th>
                      <th>{state.resx.EncryptionKey}</th>
                      <th>{state.resx.BypassIpAllowList}</th>
                      <th>{state.resx.Action}</th>
                    </tr>
                  </thead>
                  <tbody>
                    {this.users.map(user => (
                      <tr>
                        <td>{user.name}</td>
                        <td>{user.apiKey}</td>
                        <td>{user.encryptionKey}</td>
                        <td>{String(user.bypassIPWhitelist)}</td>
                        <td>
                          <dnn-button
                            appearance="danger"
                            size="small"
                            onClick={() => {
                              this.deleteUser(user).catch(console.error);
                              return;
                            }}
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
        </div>
        <dnn-modal ref={el => (this.newUserModal = el)}>
          <form
            class="create-user"
            onSubmit={event => {
              event.preventDefault();
              this.createUser(this.newUser).catch(console.error);
              return;
            }}
          >
            <h4>{state.resx.NewApiUser}</h4>
            <dnn-input
              type="text"
              label={state.resx.ApiUserNameText}
              helpText={state.resx.ApiUserNameHelp}
              required
              value={this.newUser.name}
              onValueInput={e => (this.newUser = { ...this.newUser, name: e.detail as string })}
            />
            <dnn-input
              type="date"
              label={state.resx.ApiUserExpiresOnText}
              helpText={state.resx.ApiUserExpiresOnHelp}
              required
              min={toISODate(new Date())}
              max={toISODate(addYear(addYear(new Date())))}
              value={toISODate(this.newUser.expiresOn)}
              onValueInput={e => (this.newUser = { ...this.newUser, expiresOn: new Date(e.detail as string) })}
            />
            <label>
              <dnn-checkbox
                checked={this.newUser.bypassIPWhitelist ? 'checked' : 'unchecked'}
                onCheckedchange={e => (this.newUser = { ...this.newUser, bypassIPWhitelist: e.detail === 'checked' })}
              />
              {state.resx.BypassIpAllowList}
            </label>
            <dnn-button type="submit">{state.resx.Create}</dnn-button>
          </form>
        </dnn-modal>
      </Host>
    );
  }
}
