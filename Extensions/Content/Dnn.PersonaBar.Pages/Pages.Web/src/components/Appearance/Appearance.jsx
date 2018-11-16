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
        const { page, onRetrieveThemes, onRetrieveThemeFiles } = this.props;

        onRetrieveThemes().then(data => {
			if(!data || data.success === false) {
				return;
			}
			const { defaultPortalThemeName, defaultPortalThemeLevel } = this.props;
			const selectedThemeName = page.themeName || defaultPortalThemeName;
			const selectedThemeLevel = page.themeLevel || defaultPortalThemeLevel;
			if (selectedThemeName) {
				onRetrieveThemeFiles(selectedThemeName, selectedThemeLevel);
			}
		});
    }

    componentWillReceiveProps(newProps) {
        const { page, containers, defaultPortalThemeName, defaultPortalThemeLevel, onRetrieveThemeFiles } = this.props;

        if (newProps.containers !== containers) {
            this.autoSelectFirstContainerIfNoOneIsSelected(newProps.page, newProps.containers);
        }

        if (page.themeName) {
            return;
        }

        if (defaultPortalThemeName !== newProps.defaultPortalThemeName && newProps.defaultPortalThemeName !== null) {
            onRetrieveThemeFiles(newProps.defaultPortalThemeName,newProps.defaultPortalThemeLevel);
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
        const skinSrcQueryString = this.trimAscxExtension(skinSrc);
        const containerSrcQueryString = this.trimAscxExtension(containerSrc);
        return utils.url.appendQueryString(pageUrl, {
            SkinSrc: skinSrcQueryString,
            ContainerSrc: containerSrcQueryString
        });
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
        this.props.onChangeField("themeLevel", theme.level);
        const skinSrc = this.addAscxExtension(theme.defaultThemeFile);
        this.props.onChangeField("skinSrc", skinSrc);
        this.props.onRetrieveThemeFiles(theme.packageName, theme.level);
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
        const { page, themes, layouts, containers, defaultPortalThemeName, defaultPortalThemeLevel, defaultPortalLayout, defaultPortalContainer } = this.props;
        const selectedThemeName = page.themeName || defaultPortalThemeName;
        const selectedThemeLevel = page.themeLevel || defaultPortalThemeLevel;
        const selectedTheme = themes.find(t => t.packageName === selectedThemeName && t.level === selectedThemeLevel);
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
                        defaultPortalThemeLevel={defaultPortalThemeLevel}
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
    defaultPortalThemeName: PropTypes.string,
    defaultPortalThemeLevel: PropTypes.number
};

function mapStateToProps(state) {
    return {
        themes: state.theme.themes,
        defaultPortalThemeName: state.theme.defaultPortalThemeName,
        defaultPortalThemeLevel: state.theme.defaultPortalThemeLevel,
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
