import { Component, Host, h, State, Element } from '@stencil/core';
import { IStylesResx } from '../../window.dnn';
import StylesClient, { IDnnStyles } from '../../clients/styles-client';

@Component({
  tag: 'dnn-styles-module',
  styleUrl: 'dnn-styles-module.scss',
  shadow: true,
})
export class DnnStylesModule {
  @State() resx: IStylesResx;
  @State() styles: IDnnStyles;

  @Element() el!: HTMLDnnStylesModuleElement;

  private stylesClient: StylesClient;
  private resizeObserver: ResizeObserver;
  private componentWidth: number;

  constructor() {
    this.stylesClient = new StylesClient();
  }

  connectedCallback() {
    this.resizeObserver = new ResizeObserver(entries => {
      for (let entry of entries) {
        const newWidth = entry.contentRect.width;
        if (this.componentWidth == newWidth) {
          return;
        }
        this.componentWidth = newWidth;
        this.el.classList.remove("small", "medium", "large");
        if (newWidth < 576) {
          this.el.classList.add("small");
        } else if (newWidth < 992) {
          this.el.classList.add("medium");
        } else {
          this.el.classList.add("large");
        }
      }
    });
    this.resizeObserver.observe(this.el);
  }

  disconnectedCallback() {
    this.resizeObserver.unobserve(this.el);
  }

  componentWillLoad() {
    var dnnStyles = window.dnn as unknown as IDnnWrapper;
    this.resx = dnnStyles.initStyles().utility?.resx?.Styles as IStylesResx;
    const header = document.querySelector("#dnnStylesHeader h3");
    if (header) {
      header.textContent = this.resx.nav_Styles;
    }
    this.stylesClient.getStyles()
      .then(response => this.styles = response)
      .catch(error => console.error(error));
  }

  render() {
    return (
      <Host>
        <form>
          <p>{this.resx?.ModuleDescription}</p>
          {this.styles && this.resx &&
            <div class="sections">
              <div class="section">
                <h3>{this.resx?.BrandColors}</h3>
                <dnn-color-input
                  label={this.resx?.PrimaryColor}
                  helpText={this.resx?.PrimaryColorHelp}
                  color={this.styles?.ColorPrimary.HexValue}
                  contrastColor={this.styles?.ColorPrimaryContrast.HexValue}
                  lightColor={this.styles?.ColorPrimaryLight.HexValue}
                  darkColor={this.styles?.ColorPrimaryDark.HexValue}
                  useContrastColor
                  useLightColor
                  useDarkColor
                  onColorChange={e => console.log(e.detail)}
                />
                <dnn-color-input
                  label={this.resx?.SecondaryColor}
                  helpText={this.resx?.PrimaryColorHelp}
                  color={this.styles?.ColorSecondary.HexValue}
                  contrastColor={this.styles?.ColorSecondaryContrast.HexValue}
                  lightColor={this.styles?.ColorSecondaryLight.HexValue}
                  darkColor={this.styles?.ColorSecondaryDark.HexValue}
                  useContrastColor
                  useLightColor
                  useDarkColor
                  onColorChange={e => console.log(e.detail)}
                />
                <dnn-color-input
                  label={this.resx?.TertiaryColor}
                  helpText={this.resx?.TertiaryColorHelp}
                  color={this.styles?.ColorTertiary.HexValue}
                  contrastColor={this.styles?.ColorTertiaryContrast.HexValue}
                  lightColor={this.styles?.ColorTertiaryLight.HexValue}
                  darkColor={this.styles?.ColorTertiaryDark.HexValue}
                  useContrastColor
                  useLightColor
                  useDarkColor
                  onColorChange={e => console.log(e.detail)}
                />
              </div>
              <div class="section">
                <h3>{this.resx?.ActionColors}</h3>
                <dnn-color-input
                  label={this.resx?.InformationColor}
                  helpText={this.resx?.InformationColorHelp}
                  color={this.styles?.ColorInfo.HexValue}
                  contrastColor={this.styles?.ColorInfoContrast.HexValue}
                  lightColor={this.styles?.ColorInfoLight.HexValue}
                  darkColor={this.styles?.ColorInfoDark.HexValue}
                  useContrastColor
                  useLightColor
                  useDarkColor
                  onColorChange={e => console.log(e.detail)}
                />
                <dnn-color-input
                  label={this.resx?.SuccessColor}
                  helpText={this.resx?.SuccessColorHelp}
                  color={this.styles?.ColorSuccess.HexValue}
                  contrastColor={this.styles?.ColorSuccessContrast.HexValue}
                  lightColor={this.styles?.ColorSuccessLight.HexValue}
                  darkColor={this.styles?.ColorSuccessDark.HexValue}
                  useContrastColor
                  useLightColor
                  useDarkColor
                  onColorChange={e => console.log(e.detail)}
                />
                <dnn-color-input
                  label={this.resx?.WarningColor}
                  helpText={this.resx?.WarningColorHelp}
                  color={this.styles?.ColorWarning.HexValue}
                  contrastColor={this.styles?.ColorWarningContrast.HexValue}
                  lightColor={this.styles?.ColorWarningLight.HexValue}
                  darkColor={this.styles?.ColorWarningDark.HexValue}
                  useContrastColor
                  useLightColor
                  useDarkColor
                  onColorChange={e => console.log(e.detail)}
                />
                <dnn-color-input
                  label={this.resx?.DangerColor}
                  helpText={this.resx?.DangerColorHelp}
                  color={this.styles?.ColorDanger.HexValue}
                  contrastColor={this.styles?.ColorDangerContrast.HexValue}
                  lightColor={this.styles?.ColorDangerLight.HexValue}
                  darkColor={this.styles?.ColorDangerDark.HexValue}
                  useContrastColor
                  useLightColor
                  useDarkColor
                  onColorChange={e => console.log(e.detail)}
                />
              </div>
              <div class="section">
                <h3>{this.resx?.General}</h3>
                <dnn-color-input
                  label={this.resx?.BackgroundColor}
                  helpText={this.resx?.BackgroundColorHelp}
                  color={this.styles?.ColorBackground.HexValue}
                  contrastColor={this.styles?.ColorBackgroundContrast.HexValue}
                  lightColor={this.styles?.ColorBackgroundLight.HexValue}
                  darkColor={this.styles?.ColorBackgroundDark.HexValue}
                  useContrastColor
                  useLightColor
                  useDarkColor
                  onColorChange={e => console.log(e.detail)}
                />
                <dnn-color-input
                  label={this.resx?.ForegroundColor}
                  helpText={this.resx?.ForegroundColorHelp}
                  color={this.styles?.ColorForeground.HexValue}
                  contrastColor={this.styles?.ColorForegroundContrast.HexValue}
                  lightColor={this.styles?.ColorForegroundLight.HexValue}
                  darkColor={this.styles?.ColorForegroundDark.HexValue}
                  useContrastColor
                  useLightColor
                  useDarkColor
                  onColorChange={e => console.log(e.detail)}
                />
                <dnn-color-input
                  label={this.resx?.NeutralColor}
                  helpText={this.resx?.NeutralColorHelp}
                  color={this.styles?.ColorNeutral.HexValue}
                  contrastColor={this.styles?.ColorNeutralContrast.HexValue}
                  lightColor={this.styles?.ColorNeutralLight.HexValue}
                  darkColor={this.styles?.ColorNeutralDark.HexValue}
                  useContrastColor
                  useLightColor
                  useDarkColor
                  onColorChange={e => console.log(e.detail)}
                />
                <dnn-color-input
                  label={this.resx?.SurfaceColor}
                  helpText={"Not implemented yet..."}
                />
                <dnn-input
                  type="number"
                  min={0}
                  max={100}
                  required
                  label={this.resx?.ColorVariationOpacity}
                  helpText={"not implemented yet..."}
                />
              </div>
              <div class="section">
                <h3>{this.resx?.Controls}</h3>
                <dnn-input
                  type="number"
                  min={0}
                  required
                  label={this.resx?.ControlsRadius}
                  helpText={this.resx?.ControlsRadiusHelp}
                  value={this.styles.ControlsRadius}
                />
                <dnn-input
                  type="number"
                  min={0}
                  required
                  label={this.resx?.ControlsPadding}
                  helpText={this.resx?.ControlsPaddingHelp}
                  value={this.styles.ControlsPadding}
                />
                <h3>{this.resx.Typography}</h3>
                <dnn-input
                  type="number"
                  min={0}
                  required
                  label={this.resx?.BaseFontSize}
                  helpText={this.resx?.BaseFontSizeHelp}
                  value={this.styles.BaseFontSize}
                />
              </div>
            </div>
          }
          <div class="controls">
            <dnn-button
              reversed
              onClick={() => alert("Not implemented yet...")}
            >
              {this.resx?.RestoreDefault}
            </dnn-button>
            <dnn-button
              onClick={() => alert("Not implemented yet...")}
            >
              {this.resx?.Save}
            </dnn-button>
          </div>
        </form>
      </Host>
    );
  }

}
