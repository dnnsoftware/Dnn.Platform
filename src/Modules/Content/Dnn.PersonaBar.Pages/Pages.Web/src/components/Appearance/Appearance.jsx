import React, {Component, PropTypes} from "react";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import GridCell from "dnn-grid-cell";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import Button from "dnn-button";
import Localization from "../../localization";
import ThemeSelector from "./ThemeSelector/ThemeSelector";
import LayoutSelector from "./LayoutSelector/LayoutSelector";
import ContainerSelector from "./ContainerSelector/ContainerSelector";
import ThemeActions from "../../actions/themeActions";
import style from "./style.less";

class Appearance extends Component {

    componentWillMount() {
        this.props.onRetrieveThemes();
        this.props.onRetrieveThemeFiles("Xcillion");
    }

    previewPage() {
        const { page } = this.props;
        const skinSrc = this.trimAscxExtension(page.skinSrc);
        const containerSrc = this.trimAscxExtension(page.containerSrc);
        const previewUrl = page.absoluteUrl + "?SkinSrc=" + skinSrc + "&ContainerSrc=" + containerSrc;
        window.open(previewUrl);
    }

    onSelectTheme(theme) {
        this.props.onRetrieveThemeFiles(theme.packageName);
        this.props.onChangeField("skinSrc", theme.defaultThemeFile);
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
        return value.replace(".ascx", "");
    }

    addAscxExtension(value) {
        return value + ".ascx";
    }

    onChangePageStyleSheet(event) {
        const value = event.target.value;
        this.props.onChangeField("pageStyleSheet", value);
    }

    render() {   
        const { page, themes, layouts, containers } = this.props;
        const selectedLayout = layouts.find(l => this.addAscxExtension(l.path) === page.skinSrc);
        const selectedTheme = selectedLayout ? themes.find(t => t.packageName === selectedLayout.themeName) : null;
        const selectedContainer = containers.find(c => this.addAscxExtension(c.path) === page.containerSrc);     
        return (
            <div className={style.moduleContainer}>
                <GridCell>
                    <ThemeSelector 
                        themes={themes}
                        selectedTheme={selectedTheme}
                        onSelectTheme={this.onSelectTheme.bind(this)} />
                </GridCell>
                <GridCell>
                    <LayoutSelector 
                        layouts={layouts}
                        selectedLayout={selectedLayout}
                        onSelectLayout={this.onSelectLayout.bind(this)} />
                </GridCell>
                <GridCell>
                    <ContainerSelector 
                        containers={containers}
                        selectedContainer={selectedContainer}
                        onSelectContainer={this.onSelectContainer.bind(this)} />
                </GridCell>
                <GridCell>
                    <GridCell columnSize="50">
                        <SingleLineInputWithError
                            label={Localization.get("PageStyleSheet")}
                            tooltipMessage={Localization.get("PageStyleSheetTooltip")}
                            value={page.pageStyleSheet} 
                            onChange={this.onChangePageStyleSheet.bind(this)} />
                    </GridCell>
                    <GridCell columnSize="50">
                        <Button type="secondary" onClick={this.previewPage.bind(this)} style={{marginTop: "25px", float: "right"}}>
                            {Localization.get("PreviewThemeLayoutAndContainer")}
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
    containers: PropTypes.array.isRequired
};

function mapStateToProps(state) {
    return {
        themes: state.theme.themes,
        layouts: state.theme.layouts,
        containers: state.theme.containers
    };
}

function mapDispatchToProps(dispatch) {
    return bindActionCreators ({
        onRetrieveThemes: ThemeActions.retrieveThemes,
        onRetrieveThemeFiles: ThemeActions.retrieveThemeFiles
    }, dispatch);
}

export default connect(mapStateToProps, mapDispatchToProps)(Appearance);
