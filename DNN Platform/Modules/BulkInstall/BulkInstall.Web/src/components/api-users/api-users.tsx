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
                    <label class="label">Name</label>
                    <div class="input">
                      <input type="text" title="Name" placeholder="Enter API User name" value={this.newUser.name} />
                    </div>
                  </div>
                  <div class="form-group">
                    <label class="label">Bypass IP Allow List</label>
                    <div class="input">
                      <input type="checkbox" title="Bypass IP Allow List" checked={this.newUser.bypassIPWhitelist} />
                    </div>
                  </div>
                  <div class="form-group form-group-last">
                    <div class="input offset-by-label">
                      <button type="button" class="button" onClick={() => this.createUser(this.newUser)}>Create</button>
                    </div>
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
                        <td><button type="button" class="button button-danger" onClick={() => this.deleteUser(user)}>Delete</button></td>
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
