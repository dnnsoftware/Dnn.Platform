import { Component, Element, Host, h, State } from '@stencil/core';
import store from '../../../stores/store';
import { EventLogClient, Pagination } from '../../../clients/event-log-client';
import { Event } from './dnn-bi-logs.model';
import { eventLogSeverity, EventLogSeverityInfo } from '../../../enums/EventLogSeverity';

@Component({
  tag: 'dnn-bi-logs',
  styleUrl: 'dnn-bi-logs.scss',
  shadow: true,
})
export class DnnBiLogs {
  @Element() hostElement!: HTMLDnnBiLogsElement;

  @State() private events: Event[] = [];
  @State() private eventTypes: string[] = [];
  @State() private pagination?: Pagination;
  @State() private severityFilter?: EventLogSeverityInfo;
  @State() private eventTypeFilter?: string;

  private eventLogClient: EventLogClient;
  private tabVisibilityObserver?: MutationObserver;
  private tabWasVisible = false;

  constructor() {
    this.eventLogClient = new EventLogClient();
  }

  async componentWillLoad() {
    try {
      const { data, pagination } = await this.eventLogClient.browse();
      this.events = data;
      this.pagination = pagination;
      this.eventTypes = await this.eventLogClient.getEventTypes();
    } catch (error) {
      console.error(error);
    }
  }

  componentDidLoad() {
    this.tabWasVisible = this.isTabVisible();
    const parentTab = this.hostElement.closest('dnn-tab') as HTMLElement | null;

    if (parentTab !== null && parentTab.shadowRoot !== null) {
      this.tabVisibilityObserver = new MutationObserver(() => {
        this.handleTabVisibilityChange().catch(console.error);
      });

      this.tabVisibilityObserver.observe(parentTab.shadowRoot, {
        childList: true,
        subtree: true,
      });
    }
  }

  disconnectedCallback() {
    this.tabVisibilityObserver?.disconnect();
  }

  private async setSeverityFilter(key: string) {
    this.severityFilter = eventLogSeverity.fromKey(key);
    await this.loadPage(0);
  }

  private async setEventTypeFilter(eventType: string) {
    this.eventTypeFilter = eventType;
    await this.loadPage(0);
  }

  private async loadPage(pageIndex: number) {
    const { data, pagination } = await this.eventLogClient.browse(pageIndex, this.severityFilter, this.eventTypeFilter);
    this.events = data;
    this.pagination = pagination;
  }

  private isTabVisible(): boolean {
    const parentTab = this.hostElement.closest('dnn-tab') as HTMLElement | null;
    return parentTab !== null && parentTab.shadowRoot !== null && parentTab.shadowRoot.querySelector('slot') !== null;
  }

  private async handleTabVisibilityChange() {
    const isVisible = this.isTabVisible();
    if (isVisible && !this.tabWasVisible) {
      await this.loadPage(this.pagination?.currentPage ?? 0);
    }

    this.tabWasVisible = isVisible;
  }

  private static formatDate(date: Date): string {
    const formatter = new Intl.DateTimeFormat(
      undefined,
      {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit'
      });
    return formatter.format(date);
  }

  private adjustMessage(message: string) {
    return message.replace(/\\/g, "\\<wbr>");
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
                <div class="filters">
                  <dnn-select label={store.resx.Severity} onValueChange={e => void this.setSeverityFilter(e.detail).catch(console.error)}>
                    <option value="">{store.resx.All}</option>
                    <option value={eventLogSeverity.info.eventLogSeverityKey}>{eventLogSeverity.info.localizedName}</option>
                    <option value={eventLogSeverity.warning.eventLogSeverityKey}>{eventLogSeverity.warning.localizedName}</option>
                    <option value={eventLogSeverity.alert.eventLogSeverityKey}>{eventLogSeverity.alert.localizedName}</option>
                    <option value={eventLogSeverity.critical.eventLogSeverityKey}>{eventLogSeverity.critical.localizedName}</option>
                  </dnn-select>
                  <dnn-select label={store.resx.Type} onValueChange={e => void this.setEventTypeFilter(e.detail).catch(console.error)}>
                    <option value="">{store.resx.All}</option>
                    {this.eventTypes.map(eventType => (
                      <option>{eventType}</option>
                    ))}
                  </dnn-select>
                </div>
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
                        <td innerHTML={this.adjustMessage(event.message)}></td>
                      </tr>
                    ))}
                  </tbody>
                </table>
                {this.pagination != null &&
                  <dnn-bi-log-pagination pagination={this.pagination} onPageSelected={e => void this.loadPage(e.detail).catch(console.error)} />                
                }
              </div>
            </div>
          </div>
        </div>
      </Host>
    );
  }
}
