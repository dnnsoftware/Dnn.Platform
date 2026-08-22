import { Component, Host, h, State } from '@stencil/core';
import store from '../../../stores/store';
import { ApiUserClient } from '../../../clients/api-user-client';
import { User } from './dnn-bi-api-users.model';

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
  @State() private createdUserCredentials: User | null = null;

  private newUserModal?: HTMLDnnModalElement;
  private credentialsModal?: HTMLDnnModalElement;

  private apiUserClient: ApiUserClient;

  constructor() {
    this.apiUserClient = new ApiUserClient(store.moduleId);
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
    this.newUser = {
      name: '',
      bypassIPWhitelist: false,
      expiresOn: addYear(new Date()),
    };
    await this.newUserModal?.hide();
    this.createdUserCredentials = createdUser;
    await this.credentialsModal?.show();
  }

  private async onCredentialsDismissed(): Promise<void> {
    this.createdUserCredentials = null;
    const { users } = await this.apiUserClient.getAll();
    this.users = users;
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
              <h3 class="danger">{store.resx.ApiAuthDisabled}</h3>
            </div>
          </div>
        )}
        <div class="row">
          <div class="col">
            <div class="panel">
              <div class="panel-heading">
                <h3 class="panel-title">{store.resx.ApiUsers}</h3>
                {this.enabled &&
                  <dnn-button
                    size="small"
                    onClick={() => {
                      this.newUserModal?.show().catch(console.error);
                      return;
                    }}
                  >
                    {store.resx.NewApiUser}
                  </dnn-button>
                }
              </div>
              <div class="panel-body">
                <table class="table">
                  <thead>
                    <tr>
                      <th>{store.resx.Name}</th>
                      <th>{store.resx.BypassIpAllowList}</th>
                      <th>{store.resx.Action}</th>
                    </tr>
                  </thead>
                  <tbody>
                    {this.users.map(user => (
                      <tr>
                        <td>{user.name}</td>
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
            <h4>{store.resx.NewApiUser}</h4>
            <dnn-input
              type="text"
              label={store.resx.ApiUserNameText}
              helpText={store.resx.ApiUserNameHelp}
              required
              value={this.newUser.name}
              onValueInput={e => (this.newUser = { ...this.newUser, name: e.detail as string })}
            />
            <dnn-input
              type="date"
              label={store.resx.ApiUserExpiresOnText}
              helpText={store.resx.ApiUserExpiresOnHelp}
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
              {store.resx.BypassIpAllowList}
            </label>
            <dnn-button type="submit">{store.resx.Create}</dnn-button>
          </form>
        </dnn-modal>
        <dnn-modal
          ref={el => (this.credentialsModal = el)}
          onDismissed={() => { this.onCredentialsDismissed().catch(console.error); }}
          preventBackdropDismiss
        >
          {this.createdUserCredentials && (
            <div class="credentials">
              <h4>{store.resx.NewApiUserCredentialsTitle}</h4>
              <p class="credentials-warning">{store.resx.NewApiUserCredentialsWarning}</p>
              <div class="credential-row">
                <span class="credential-label">{store.resx.ApiKey}</span>
                <code class="credential-value">{this.createdUserCredentials.apiKey}</code>
                <dnn-button
                  size="small"
                  onClick={() => {
                    navigator.clipboard.writeText(this.createdUserCredentials?.apiKey ?? "").catch(console.error);
                    return;
                  }}
                >
                  {store.resx.Copy}
                </dnn-button>
              </div>
              <div class="credential-row">
                <span class="credential-label">{store.resx.EncryptionKey}</span>
                <code class="credential-value">{this.createdUserCredentials.encryptionKey}</code>
                <dnn-button
                  size="small"
                  onClick={() => {
                    navigator.clipboard.writeText(this.createdUserCredentials?.encryptionKey ?? "").catch(console.error);
                    return;
                  }}
                >
                  {store.resx.Copy}
                </dnn-button>
              </div>
            </div>
          )}
        </dnn-modal>
      </Host>
    );
  }
}
