import { Component, Host, h } from '@stencil/core';
import { Event } from './bulk-install-logs.model';

@Component({
  tag: 'bulk-install-logs',
  styleUrl: 'bulk-install-logs.scss',
  shadow: true,
})
export class BulkInstallLogs {

  private events: Event[] = [];

  render() {
    return (
      <Host>
        <div class="row">
          <div class="col">
            <div class="panel">
              <div class="panel-heading">
                <h3 class="panel-title">Events</h3>
              </div>
              <div class="panel-body">
                <table class="table">
                  <thead>
                    <tr>
                      <th>Date</th>
                      <th>Type</th>
                      <th>Message</th>
                    </tr>
                  </thead>
                  <tbody>
                    {this.events.map((event) => (
                      <tr>
                        <td>{event.date}</td>
                        <td>{event.type}</td>
                        <td>{event.message}</td>
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
