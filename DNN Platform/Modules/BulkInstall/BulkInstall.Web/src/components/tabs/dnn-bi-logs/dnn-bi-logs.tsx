import { Component, Host, h, State } from '@stencil/core';
import store from '../../../stores/store';
import { EventLogClient, Pagination } from '../../../clients/event-log-client';
import { Event } from './dnn-bi-logs.model';

@Component({
  tag: 'dnn-bi-logs',
  styleUrl: 'dnn-bi-logs.scss',
  shadow: true,
})
export class DnnBiLogs {
  @State() private events: Event[] = [];
  @State() private pagination: Pagination;

  private eventLogClient: EventLogClient;

  constructor() {
    this.eventLogClient = new EventLogClient(store.moduleId);
  }

  async componentWillLoad() {
    try {
      const { data, pagination } = await this.eventLogClient.browse();
      this.events = data;
      this.pagination = pagination;
    } catch (error) {
      console.error(error);
    }
  }

  private async loadPage(pageIndex: number) {
    const { data, pagination } = await this.eventLogClient.browse(pageIndex);
    this.events = data;
    this.pagination = pagination;
  }

  private static formatDate(date: Date): string {
    const formatter = new Intl.DateTimeFormat(undefined, { dateStyle: 'long', timeStyle: 'medium' });
    return formatter.format(date);
  }

  render() {
    return (
      <Host>
        <div class="row">
          <div class="col">
            <div class="panel">
              <div class="panel-heading">
                <h3 class="panel-title">{store.resx.Events}</h3>
              </div>
              <div class="panel-body">
                <table class="table">
                  <thead>
                    <tr>
                      <th>{store.resx.Date}</th>
                      <th>{store.resx.Severity}</th>
                      <th>{store.resx.Type}</th>
                      <th>{store.resx.Message}</th>
                    </tr>
                  </thead>
                  <tbody>
                    {this.events.map(event => (
                      <tr>
                        <td>
                          <time dateTime={event.date.toISOString()}>{DnnBiLogs.formatDate(event.date)}</time>
                        </td>
                        <td>{event.severity.localizedName}</td>
                        <td>{event.type}</td>
                        <td>{event.message}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
                {this.pagination.pages > 1 && (
                  <ol class="pagination">
                    {[...Array(this.pagination.pages)].map((_, index) => (
                      <li class={index === this.pagination.currentPage ? 'active' : ''}>
                        <button
                          disabled={index === this.pagination.currentPage}
                          onClick={e => {
                            e.preventDefault();
                            this.loadPage(index).catch(console.error);
                          }}
                        >
                          {index + 1}
                        </button>
                      </li>
                    ))}
                  </ol>
                )}
              </div>
            </div>
          </div>
        </div>
      </Host>
    );
  }
}
