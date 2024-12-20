import { Component, Host, h } from '@stencil/core';
import { User } from './api-user.model';

@Component({
  tag: 'api-users',
  styleUrl: 'api-users.scss',
  shadow: true,
})
export class ApiUsers {

  private users: User[] = [];
  private newUser: User = {
    name: '',
    apiKey: '',
    encryptionKey: '',
    bypassIPWhitelist: false,
  }

  private createUser(_newUser: User): (event: MouseEvent) => void {
    alert('Method not implemented.');
    return;
  }

  private deleteUser(_user: User): (event: MouseEvent) => void {
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
                <h3 class="panel-title">New API User</h3>
              </div>
              <div class="panel-body">
                <div class="form-horizontal">
                  <div class="form-group">
                    <dnn-input type="text" label="Name" helpText="Enter API User name" required></dnn-input>
                  </div>
                  <div class="form-group">
                    <label class="label">
                      <dnn-checkbox checked={this.newUser.bypassIPWhitelist ? 'checked' : 'unchecked'}></dnn-checkbox>
                      Bypass IP Allow List
                    </label>
                  </div>
                  <div class="form-group form-group-last">
                    <dnn-button appearance="primary" onClick={() => this.createUser(this.newUser)}>Create</dnn-button>
                  </div>
                </div>
  
                <div class="clearfix"></div>
              </div>
            </div>
          </div>
  
          <div class="col">
            <div class="panel">
              <div class="panel-heading">
                <h3 class="panel-title">API Users</h3>
              </div>
              <div class="panel-body">
                <table class="table">
                  <thead>
                    <tr>
                      <th>Name</th>
                      <th>API Key</th>
                      <th>Encryption Key</th>
                      <th>Bypass IP Allow List</th>
                      <th></th>
                    </tr>
                  </thead>
                  <tbody>
                    {this.users.map((user) => (
                      <tr>
                        <td>{user.name}</td>
                        <td>{user.apiKey}</td>
                        <td>{user.encryptionKey}</td>
                        <td>{String(user.bypassIPWhitelist)}</td>
                        <td><dnn-button appearance="danger" size="small" onClick={() => this.deleteUser(user)}>Delete</dnn-button></td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>
      </Host>
    );
  }
}
