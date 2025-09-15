import { Component, Host, h, State, Element } from '@stencil/core';
import { IStylesResx } from '../../window.dnn';
import StylesClient, { IPortalStyles } from '../../clients/styles-client';
import { DnnColorInfo } from '@dnncommunity/dnn-elements/dist/types/components';
import ColorNames from './color-names';
@Component({
  tag: 'dnn-styles-module',
  styleUrl: 'dnn-styles-module.scss',
  shadow: true,
})
export class DnnStylesModule {
  @State() resx: IStylesResx;
  @State() originalStyles: IPortalStyles;
  @State() styles: IPortalStyles;
  @State() isHost: boolean = false;

  @Element() el!: HTMLDnnStylesModuleElement;

  private stylesClient: StylesClient;
  private resizeObserver: ResizeObserver;
  private componentWidth: number;

  constructor() {
    this.stylesClient = new StylesClient();
  }

  connectedCallback() {
    this.resizeObserver = new ResizeObserver(entries => {
      for (const entry of entries) {
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
    const dnnStyles = window.dnn as unknown as IDnnWrapper;
    this.resx = dnnStyles.initStyles().utility?.resx?.Styles;
    const header = document.querySelector("#dnnStylesHeader h3");
    if (header) {
      header.textContent = this.resx.nav_Styles;
    }
    this.stylesClient.getStyles()
      .then(response => {
        this.originalStyles = response;
        this.styles = {...this.originalStyles};
      })
      .catch(error => {
        alert(this.resx?.GetStylesError);
        console.error(error);
      });
    this.isHost = this.stylesClient.isHostUser;
  }

  private handleColorChange(colorName: ColorNames, detail: DnnColorInfo): void {
    this.styles = {
      ...this.styles,
      [`Color${colorName}`]: detail.color,
      [`Color${colorName}Contrast`]: detail.contrastColor,
      [`Color${colorName}Light`]: detail.lightColor,
      [`Color${colorName}Dark`]: detail.darkColor,
    }
  }

  private handleSave(): void {
    this.stylesClient.saveStyles(this.styles)
      .then(() => {
        this.originalStyles = this.styles;
        this.stylesClient.notify(this.resx?.SaveSuccess);
      })
      .catch(error => {
        this.stylesClient.notifyError(this.resx?.SaveError);
        console.error(error);
      });
  }

  private handleRestoreDefault(): void {
    this.stylesClient.restoreStyles()
    .then(response => {
      this.originalStyles = response;
      this.styles = {...this.originalStyles};
    })
    .catch(error => {
      this.stylesClient.notifyError(this.resx?.SaveError);
      console.error(error);
    });
  }

  render() {
    return (
      <Host>
        <form
          onSubmit={e => {
            e.preventDefault();
            this.handleSave();
          }}
        >
          <p>{this.resx?.ModuleDescription}</p>
          {this.styles && this.resx &&
            <div class="sections">
              <div class="section">
                <h3>{this.resx?.GeneralColors}</h3>
                <dnn-color-input
                  label={this.resx?.BackgroundColor}
                  helpText={this.resx?.BackgroundColorHelp}
                  color={this.styles?.ColorBackground}
                  contrastColor={this.styles?.ColorBackgroundContrast}
                  lightColor={this.styles?.ColorBackgroundLight}
                  darkColor={this.styles?.ColorBackgroundDark}
                  useContrastColor
                  useLightColor
                  useDarkColor
                  onColorChange={e => this.handleColorChange("Background", e.detail)}
                />
                <dnn-color-input
                  label={this.resx?.ForegroundColor}
                  helpText={this.resx?.ForegroundColorHelp}
                  color={this.styles?.ColorForeground}
                  contrastColor={this.styles?.ColorForegroundContrast}
                  lightColor={this.styles?.ColorForegroundLight}
                  darkColor={this.styles?.ColorForegroundDark}
                  useContrastColor
                  useLightColor
                  useDarkColor
                  onColorChange={e => this.handleColorChange("Foreground", e.detail)}
                />
                <dnn-color-input
                  label={this.resx?.NeutralColor}
                  helpText={this.resx?.NeutralColorHelp}
                  color={this.styles?.ColorNeutral}
                  contrastColor={this.styles?.ColorNeutralContrast}
                  lightColor={this.styles?.ColorNeutralLight}
                  darkColor={this.styles?.ColorNeutralDark}
                  useContrastColor
                  useLightColor
                  useDarkColor
                  onColorChange={e => this.handleColorChange("Neutral", e.detail)}
                />
                <dnn-color-input
                  label={this.resx?.SurfaceColor}
                  helpText={this.resx.SurfaceColorHelp}
                  color={this.styles?.ColorSurface}
                  contrastColor={this.styles?.ColorSurfaceContrast}
                  lightColor={this.styles?.ColorSurfaceLight}
                  darkColor={this.styles?.ColorSurfaceDark}
                  useContrastColor
                  useLightColor
                  useDarkColor
                  onColorChange={e => this.handleColorChange("Surface", e.detail)}
                />
              </div>
              <div class="section">
                <h3>{this.resx?.ActionColors}</h3>
                <dnn-color-input
                  label={this.resx?.InformationColor}
                  helpText={this.resx?.InformationColorHelp}
                  color={this.styles?.ColorInfo}
                  contrastColor={this.styles?.ColorInfoContrast}
                  lightColor={this.styles?.ColorInfoLight}
                  darkColor={this.styles?.ColorInfoDark}
                  useContrastColor
                  useLightColor
                  useDarkColor
                  onColorChange={e => this.handleColorChange("Info", e.detail)}
                />
                <dnn-color-input
                  label={this.resx?.SuccessColor}
                  helpText={this.resx?.SuccessColorHelp}
                  color={this.styles?.ColorSuccess}
                  contrastColor={this.styles?.ColorSuccessContrast}
                  lightColor={this.styles?.ColorSuccessLight}
                  darkColor={this.styles?.ColorSuccessDark}
                  useContrastColor
                  useLightColor
                  useDarkColor
                  onColorChange={e => this.handleColorChange("Success", e.detail)}
                />
                <dnn-color-input
                  label={this.resx?.WarningColor}
                  helpText={this.resx?.WarningColorHelp}
                  color={this.styles?.ColorWarning}
                  contrastColor={this.styles?.ColorWarningContrast}
                  lightColor={this.styles?.ColorWarningLight}
                  darkColor={this.styles?.ColorWarningDark}
                  useContrastColor
                  useLightColor
                  useDarkColor
                  onColorChange={e => this.handleColorChange("Warning", e.detail)}
                />
                <dnn-color-input
                  label={this.resx?.DangerColor}
                  helpText={this.resx?.DangerColorHelp}
                  color={this.styles?.ColorDanger}
                  contrastColor={this.styles?.ColorDangerContrast}
                  lightColor={this.styles?.ColorDangerLight}
                  darkColor={this.styles?.ColorDangerDark}
                  useContrastColor
                  useLightColor
                  useDarkColor
                  onColorChange={e => this.handleColorChange("Danger", e.detail)}
                />
              </div>
              <div class="section">
                <h3>{this.resx?.BrandColors}</h3>
                <dnn-color-input
                  label={this.resx?.PrimaryColor}
                  helpText={this.resx?.PrimaryColorHelp}
                  color={this.styles?.ColorPrimary}
                  contrastColor={this.styles?.ColorPrimaryContrast}
                  lightColor={this.styles?.ColorPrimaryLight}
                  darkColor={this.styles?.ColorPrimaryDark}
                  useContrastColor
                  useLightColor
                  useDarkColor
                  onColorChange={e => this.handleColorChange("Primary", e.detail)}
                />
                <dnn-color-input
                  label={this.resx?.SecondaryColor}
                  helpText={this.resx?.PrimaryColorHelp}
                  color={this.styles?.ColorSecondary}
                  contrastColor={this.styles?.ColorSecondaryContrast}
                  lightColor={this.styles?.ColorSecondaryLight}
                  darkColor={this.styles?.ColorSecondaryDark}
                  useContrastColor
                  useLightColor
                  useDarkColor
                  onColorChange={e => this.handleColorChange("Secondary", e.detail)}
                />
                <dnn-color-input
                  label={this.resx?.TertiaryColor}
                  helpText={this.resx?.TertiaryColorHelp}
                  color={this.styles?.ColorTertiary}
                  contrastColor={this.styles?.ColorTertiaryContrast}
                  lightColor={this.styles?.ColorTertiaryLight}
                  darkColor={this.styles?.ColorTertiaryDark}
                  useContrastColor
                  useLightColor
                  useDarkColor
                  onColorChange={e => this.handleColorChange("Tertiary", e.detail)}
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
                  onValueChange={e => this.styles={...this.styles, ControlsRadius: e.detail as number}}
                />
                <dnn-input
                  type="number"
                  min={0}
                  required
                  label={this.resx?.ControlsPadding}
                  helpText={this.resx?.ControlsPaddingHelp}
                  value={this.styles.ControlsPadding}
                  onValueChange={e => this.styles={...this.styles, ControlsPadding: e.detail as number}}
                />
              </div>
              <div class="section">
                <h3>{this.resx?.Colors}</h3>
                <dnn-input
                  type="number"
                  min={0}
                  max={1}
                  step={0.01}
                  required
                  label={this.resx?.ColorVariationOpacity}
                  helpText={this.resx?.ColorVariationOpacityHelp}
                  value={this.styles.VariationOpacity}
                  onValueChange={e => this.styles={...this.styles, VariationOpacity: e.detail as number}}
                />
              </div>
              <div class="section">
                  <h3>{this.resx.Typography}</h3>
                  <dnn-input
                    type="number"
                    min={0}
                    required
                    label={this.resx?.BaseFontSize}
                    helpText={this.resx?.BaseFontSizeHelp}
                    value={this.styles.BaseFontSize}
                    onValueChange={e => this.styles={...this.styles, BaseFontSize: e.detail as number}}
                  />
              </div>
            </div>
          }
          {this.isHost &&
            <div class="permissions-section">
              <h3>{this.resx?.Permissions}</h3>
              <label>
                <dnn-toggle
                  checked={this.styles?.AllowAdminEdits}
                  onCheckChanged={e => this.styles={...this.styles, AllowAdminEdits: e.detail.checked}}
                />
                {this.resx?.AllowAdminEdits}
              </label>
              <em>{this.resx.AllowAdminEditsHelp}</em>
            </div>
          }
          <div class="controls">
            <dnn-button
              reversed
              onClick={() => {
                this.styles = this.originalStyles;
              }}
            >
              {this.resx?.Reset}
            </dnn-button>
            <dnn-button
              appearance="danger"
              confirm
              confirmMessage={this.resx?.RestoreDefaultMessage}
              confirmNoText={this.resx?.No}
              confirmYesText={this.resx?.Yes}
              onConfirmed={() => this.handleRestoreDefault()}
            >
              {this.resx?.RestoreDefault}
            </dnn-button>
            <dnn-button
              type='submit'
            >
              {this.resx?.Save}
            </dnn-button>
          </div>
        </form>
      </Host>
    );
  }
}
