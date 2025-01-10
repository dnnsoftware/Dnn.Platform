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
  
  private newUserModal: HTMLDnnModalElement;

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
            <div class="button-row">
              <dnn-button size="small" onClick={() => this.newUserModal.show()}>New API User</dnn-button>
            </div>
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
                      <th>Action</th>
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
        <dnn-modal
          ref={(el) => this.newUserModal = el}
          backdropDismiss
        >
          <form
            class="create-user"
            onSubmit={(event) => {
              event.preventDefault();
              this.createUser(this.newUser);
            }}
          >
            <h4>New API User</h4>
            <dnn-input
              type="text"
              label="Name"
              helpText="Enter API User Name"
              required
            />
            <label>
              <dnn-checkbox checked={this.newUser.bypassIPWhitelist ? 'checked' : 'unchecked'}></dnn-checkbox>
              Bypass IP Allow List
            </label>
            <dnn-button formButtonType="submit">Create</dnn-button>
          </form>
        </dnn-modal>
      </Host>
    );
  }
}
