import { Component, h, Host, Prop, Fragment } from '@stencil/core';
import store from '../../../../stores/store';
import { PackageJob } from '../dnn-bi-install.model';

@Component({
  tag: 'dnn-bi-package-job',
  styleUrl: 'dnn-bi-package-job.scss',
  shadow: true,
})
export class DnnBiPackageJob {
  /** The package job. */
  @Prop() job!: PackageJob;

  /** Whether the installation was completed. */
  @Prop() attempted!: boolean;

  render() {
    return (
      <Host>
        <h4 class={this.job.canInstall ? 'valid' : 'invalid'}>
          {!this.job.canInstall && <dnn-bi-circle-x-icon />}
          {this.job.name} <span class="version">{this.job.version}</span>
        </h4>
        {!this.attempted && this.job.dependencies.length > 0 && (
          <summary>
            <detail>{store.resx.PackageDependencies}</detail>
            <ul>
              {this.job.dependencies.map(dependency => (
                <li class={dependency.isMet ? 'success' : 'failure'}>
                  {dependency.isMet && <dnn-bi-checkmark-icon />}
                  {!dependency.isMet && <dnn-bi-circle-x-icon />}
                  {dependency.isCoreVersionDependency && (
                    <>
                      {store.resx.PlatformVersion} <span class="version">{dependency.packageName}</span>
                    </>
                  )}
                  {!dependency.isCoreVersionDependency && (
                    <>
                      {dependency.packageName} <span class="version">{dependency.dependencyVersion}</span>
                    </>
                  )}
                </li>
              ))}
            </ul>
          </summary>
        )}
      </Host>
    );
  }
}
