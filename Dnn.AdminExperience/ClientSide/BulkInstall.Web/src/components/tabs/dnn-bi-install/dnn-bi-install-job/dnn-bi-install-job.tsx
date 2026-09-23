import { Component, h, Host, Prop } from '@stencil/core';
import store from '../../../../stores/store';
import { InstallJob } from '../dnn-bi-install.model';

@Component({
  tag: 'dnn-bi-install-job',
  styleUrl: 'dnn-bi-install-job.scss',
  shadow: true,
})
export class DnnBiInstallJob {
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
            {this.job.failures.length > 0 && (
              <ul class="failure">
                {this.job.failures.map(failure => (
                  <li>{failure}</li>
                ))}
              </ul>
            )}
            {this.job.packages.length === 1 && <dnn-bi-package-job job={this.job.packages[0]} attempted={this.job.attempted} />}
            {this.job.packages.length !== 1 && (
              <details>
                <summary>
                  {this.job.packages.length} {store.resx.Packages}
                </summary>
                <ul>
                  {this.job.packages.map(p => (
                    <li>
                      <dnn-bi-package-job job={p} attempted={this.job.attempted} />
                    </li>
                  ))}
                </ul>
              </details>
            )}
          </div>
          {!this.job.attempted && this.job.canInstall && (
            <div class="pending">
              <dnn-bi-clock-icon />
            </div>
          )}
          {!this.job.attempted && !this.job.canInstall && (
            <div class="failure">
              <dnn-bi-circle-x-icon />
            </div>
          )}
          {this.job.attempted && !this.job.success && (
            <div class="failure">
              <dnn-bi-circle-x-icon />
            </div>
          )}
          {this.job.attempted && this.job.success && (
            <div class="success">
              <dnn-bi-checkmark-icon />
            </div>
          )}
        </div>
      </Host>
    );
  }
}
