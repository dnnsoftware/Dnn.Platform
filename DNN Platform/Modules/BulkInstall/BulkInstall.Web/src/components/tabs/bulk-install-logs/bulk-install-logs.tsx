import { Component, Host, h } from '@stencil/core';
import { Event } from './bulk-install-logs.model';
import state from '../../../stores/store';
import { EventLogClient } from '../../../clients/event-log-client';

@Component({
  tag: 'bulk-install-logs',
  styleUrl: 'bulk-install-logs.scss',
  shadow: true,
})
export class BulkInstallLogs {
  private events: Event[] = [];

  private eventLogClient: EventLogClient;

  constructor() {
    this.eventLogClient = new EventLogClient(state.moduleId);
  }

  async componentWillLoad() {
    try {
      const browseResponse = await this.eventLogClient.browse();
      this.events = browseResponse.Data;
    } catch (error) {
      console.error(error);
    }
  }

  private static formatDate(event: Event): string {
    const formatter = new Intl.DateTimeFormat(undefined, { dateStyle: 'long', timeStyle: 'medium' });
    return formatter.format(event.date);
  }

  render() {
    return (
      <Host>
        <div class="row">
          <div class="col">
            <div class="panel">
              <div class="panel-heading">
                <h3 class="panel-title">{state.resx.Events}</h3>
              </div>
              <div class="panel-body">
                <table class="table">
                  <thead>
                    <tr>
                      <th>{state.resx.Date}</th>
                      <th>{state.resx.Severity}</th>
                      <th>{state.resx.Type}</th>
                      <th>{state.resx.Message}</th>
                    </tr>
                  </thead>
                  <tbody>
                    {this.events.map(event => (
                      <tr>
                        <td>{BulkInstallLogs.formatDate(event)}</td>
                        <td>{event.severity.localizedName}</td>
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
