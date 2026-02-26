import { Component, h, Host, Prop } from '@stencil/core';
import store from '../../../../stores/store';
import { PackageJob } from '../bulk-install-install.model';

@Component({
  tag: 'bulk-install-package-job',
  styleUrl: 'bulk-install-package-job.scss',
  shadow: true,
})
export class BulkInstallPackageJob {
  /** The package job. */
  @Prop() job!: PackageJob;

  render() {
    return (
      <Host>
        <h4 class={this.job.canInstall ? 'valid' : 'invalid'}>
          {!this.job.canInstall && <bulk-install-circle-x-icon />}
          {this.job.name} <span class="version">{this.job.version}</span>
        </h4>
        {this.job.dependencies.length > 0 && (
          <summary>
            <detail>{store.resx.PackageDependencies}</detail>
            <ul>
              {this.job.dependencies.map(dependency => (
                <li class={dependency.isMet ? 'success' : 'failure'}>
                  {dependency.isMet && <bulk-install-checkmark-icon />}
                  {!dependency.isMet && <bulk-install-circle-x-icon />}
                  {dependency.packageName} <span class="version">{dependency.dependencyVersion}</span>
                </li>
              ))}
            </ul>
          </summary>
        )}
      </Host>
    );
  }
}
