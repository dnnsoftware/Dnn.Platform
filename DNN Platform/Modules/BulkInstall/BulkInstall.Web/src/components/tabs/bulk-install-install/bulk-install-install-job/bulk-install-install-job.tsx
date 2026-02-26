import { Component, h, Host, Prop } from '@stencil/core';
import state from '../../../../stores/store';
import { InstallJob } from '../bulk-install-install.model';

@Component({
  tag: 'bulk-install-install-job',
  styleUrl: 'bulk-install-install-job.scss',
  shadow: true,
})
export class BulkInstallInstallJob {
  /** The install job. */
  @Prop() job!: InstallJob;

  render() {
    return (
      <Host>
        <div class="container">
          <div class="preview">
            <img src="/Icons/Sigma/ExtZip_32X32_Standard.png" alt={this.job.name} />
          </div>
          <div class="file">
            <span>{this.job.name}</span>
            <div class="progress">
              <div class="progress-bar" style={{ width: `100%` }}></div>
          {!this.job.attempted && (
            <div class="pending">
              <bulk-install-clock-icon />
            </div>
          )}
          {this.job.attempted && !this.job.success && (
            <div class="failure">
              <bulk-install-circle-x-icon />
            </div>
          )}
          {this.job.attempted && this.job.success && (
            <div class="success">
              <bulk-install-checkmark-icon />
            </div>
          )}
        </div>
      </Host>
    );
  }
}
