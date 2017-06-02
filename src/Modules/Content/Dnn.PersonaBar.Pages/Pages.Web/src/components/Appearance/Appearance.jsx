import React, {Component, PropTypes} from "react";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import GridCell from "dnn-grid-cell";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import Button from "dnn-button";
import Localization from "../../localization";
import utils from "../../utils";
import ThemeSelector from "./ThemeSelector/ThemeSelector";
import LayoutSelector from "./LayoutSelector/LayoutSelector";
import ContainerSelector from "./ContainerSelector/ContainerSelector";
import ThemeActions from "../../actions/themeActions";
import style from "./style.less";

class Appearance extends Component {

    componentWillMount() {
        const { page, defaultPortalThemeName, onRetrieveThemes, onRetrieveThemeFiles } = this.props;

        onRetrieveThemes();

        const selectedThemeName = page.themeName || defaultPortalThemeName;
        if (selectedThemeName) {
            onRetrieveThemeFiles(selectedThemeName);
        }
    }

    componentWillReceiveProps(newProps) {
        const { page, containers, defaultPortalThemeName, onRetrieveThemeFiles } = this.props;

        if (newProps.containers !== containers) {
            this.autoSelectFirstContainerIfNoOneIsSelected(newProps.page, newProps.containers);
        }

        if (page.themeName) {
            return;
        }

        if (defaultPortalThemeName !== newProps.defaultPortalThemeName && newProps.defaultPortalThemeName !== null) {
            onRetrieveThemeFiles(newProps.defaultPortalThemeName);
        }
    }

    autoSelectFirstContainerIfNoOneIsSelected(page, containers) {
        const { onChangeField, defaultPortalContainer } = this.props;
        const selectedContainerPath = page.containerSrc || defaultPortalContainer;
        const selectedContainer = containers.find(this.findByPath(selectedContainerPath));
        if (!selectedContainer && containers.length !== 0) {
            const container = containers[0];
            onChangeField("containerSrc", this.addAscxExtension(container.path));
        }
    }

    getPagePreviewUrl(skinSrc, containerSrc) {
        const { page } = this.props;
        // if is new page take current page loaded behind the persona bar for preview
        const pageUrl = page.tabId !== 0 ? page.absoluteUrl : window.parent.location.href;
        const skinSrcQueryString = encodeURI(this.trimAscxExtension(skinSrc));
        const containerSrcQueryString = encodeURI(this.trimAscxExtension(containerSrc));
        const queryStringSeparator = pageUrl.indexOf("?") === -1 ? "?" : "&";
        return pageUrl + queryStringSeparator + "SkinSrc=" + skinSrcQueryString + "&ContainerSrc=" + containerSrcQueryString;
    }

    previewPage() {
        const { page, defaultPortalLayout, defaultPortalContainer } = this.props;
        const skinSrc = page.skinSrc || defaultPortalLayout;
        const containerSrc = page.containerSrc || defaultPortalContainer;

        if (!skinSrc || !containerSrc) {
            utils.notify(Localization.get("PleaseSelectLayoutContainer"));
            return;
        }

        const previewUrl = this.getPagePreviewUrl(skinSrc, containerSrc);
        window.open(previewUrl);
    }

    onSelectTheme(theme) {
        this.props.onChangeField("themeName", theme.packageName);
        const skinSrc = this.addAscxExtension(theme.defaultThemeFile);
        this.props.onChangeField("skinSrc", skinSrc);
        this.props.onRetrieveThemeFiles(theme.packageName);
    }

    onSelectLayout(layout) {
        if (!layout) {
            this.props.onChangeField("skinSrc", null);
        }
        const skinSrc = this.addAscxExtension(layout.path);
        this.props.onChangeField("skinSrc", skinSrc);
    }

    onSelectContainer(container) {
        if (!container) {
            this.props.onChangeField("containerSrc", null);
        }
        const containerSrc = this.addAscxExtension(container.path);
        this.props.onChangeField("containerSrc", containerSrc);
    }

    trimAscxExtension(value) {
        if (!value) return value;
        return value.replace(".ascx", "");
    }

    addAscxExtension(value) {
        if (!value) return value;
        return value + ".ascx";
    }

    onChangePageStyleSheet(event) {
        const value = event.target.value;
        this.props.onChangeField("pageStyleSheet", value);
    }

    findByPath(componentSrc) {
        return (c) => utils.areEqualInvariantCase(this.addAscxExtension(c.path), componentSrc);
    }

    render() {
        const { page, themes, layouts, containers, defaultPortalThemeName, defaultPortalLayout, defaultPortalContainer } = this.props;
        const selectedThemeName = page.themeName || defaultPortalThemeName;
        const selectedTheme = themes.find(t => t.packageName === selectedThemeName);
        const noThemeSelected = !selectedTheme;
        const selectedLayoutPath = page.skinSrc || defaultPortalLayout;
        const selectedLayout = layouts.find(this.findByPath(selectedLayoutPath));
        const selectedContainerPath = page.containerSrc || defaultPortalContainer;
        const selectedContainer = containers.find(this.findByPath(selectedContainerPath));

        return (
            <div className={style.moduleContainer}>
                <GridCell>
                    <ThemeSelector
                        themes={themes}
                        defaultPortalThemeName={defaultPortalThemeName}
                        selectedTheme={selectedTheme}
                        onSelectTheme={this.onSelectTheme.bind(this) } />
                </GridCell>
                <GridCell>
                    <LayoutSelector
                        noThemeSelected={noThemeSelected}
                        layouts={layouts}
                        defaultPortalLayout={this.trimAscxExtension(defaultPortalLayout) }
                        selectedLayout={selectedLayout}
                        onSelectLayout={this.onSelectLayout.bind(this) } />
                </GridCell>
                <GridCell>
                    <ContainerSelector
                        noThemeSelected={noThemeSelected}
                        containers={containers}
                        defaultPortalContainer={this.trimAscxExtension(defaultPortalContainer) }
                        selectedContainer={selectedContainer}
                        onSelectContainer={this.onSelectContainer.bind(this) } />
                </GridCell>
                <GridCell>
                    <GridCell columnSize="50">
                        <SingleLineInputWithError style={{ width: "100%" }}
                            label={Localization.get("PageStyleSheet") }
                            tooltipMessage={Localization.get("PageStyleSheetTooltip") }
                            value={page.pageStyleSheet}
                            onChange={this.onChangePageStyleSheet.bind(this) } />
                    </GridCell>
                    <GridCell columnSize="50">
                        <Button type="secondary" onClick={this.previewPage.bind(this) } style={{ marginTop: "25px", float: "right" }}>
                            {Localization.get("PreviewThemeLayoutAndContainer") }
                        </Button>
                    </GridCell>
                </GridCell>
            </div>
        );
    }
}

Appearance.propTypes = {
    page: PropTypes.object.isRequired,
    onChangeField: PropTypes.func.isRequired,
    onRetrieveThemes: PropTypes.func.isRequired,
    onRetrieveThemeFiles: PropTypes.func.isRequired,
    themes: PropTypes.array.isRequired,
    layouts: PropTypes.array.isRequired,
    containers: PropTypes.array.isRequired,
    defaultPortalLayout: PropTypes.string,
    defaultPortalContainer: PropTypes.string,
    defaultPortalThemeName: PropTypes.string
};

function mapStateToProps(state) {
    return {
        themes: state.theme.themes,
        defaultPortalThemeName: state.theme.defaultPortalThemeName,
        defaultPortalLayout: state.theme.defaultPortalLayout,
        defaultPortalContainer: state.theme.defaultPortalContainer,
        layouts: state.theme.layouts,
        containers: state.theme.containers
    };
}

function mapDispatchToProps(dispatch) {
    return bindActionCreators({
        onRetrieveThemes: ThemeActions.retrieveThemes,
        onRetrieveThemeFiles: ThemeActions.retrieveThemeFiles
    }, dispatch);
}

export default connect(mapStateToProps, mapDispatchToProps)(Appearance);
