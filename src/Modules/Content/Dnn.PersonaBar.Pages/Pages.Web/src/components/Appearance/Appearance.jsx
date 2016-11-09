import React, {Component, PropTypes} from "react";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import ThemeSelector from "./ThemeSelector/ThemeSelector";
import LayoutSelector from "./LayoutSelector/LayoutSelector";
import ContainerSelector from "./ContainerSelector/ContainerSelector";
import ThemeActions from "../../actions/themeActions";

class Appearance extends Component {

    componentWillMount() {
        this.props.onRetrieveThemes();
        this.props.onRetrieveThemeFiles("Xcillion");
    }

    render() {        
        return (
            <div>
                <div>
                    <ThemeSelector themes={this.props.themes} />
                </div>
                <div>
                    <LayoutSelector layouts={this.props.layouts} />
                </div>
                <div>
                    <ContainerSelector containers={this.props.layouts} />
                </div>
            </div>
        );
    }
}

Appearance.propTypes = {
    page: PropTypes.object.isRequired,
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
