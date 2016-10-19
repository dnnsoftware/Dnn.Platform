import React, {Component, PropTypes } from "react";
import { connect } from "react-redux";
import {
    theme as ThemeActions
} from "actions";
import Localization from "localization";
import GridCell from "dnn-grid-cell";
import Button from "dnn-button";
import OverflowText from "dnn-text-overflow-wrapper";

import EmptyThumbnail from "../../EmptyThumbnail";

import "./style.less";

class Theme extends Component {
    constructor() {
        super();
        this.state = {};
    }

    selectedAsSite(){
        const {props, state} = this;
        let theme = props.theme;
        let currentTheme = props.currentTheme;

        if(theme.type === 0){
            return currentTheme.SiteLayout.themeName === theme.packageName;
        } else {
            return currentTheme.SiteContainer.themeName === theme.packageName;
        }
    }

    getClassName(){
        const {props, state} = this;
        let theme = props.theme;
        let currentTheme = props.currentTheme;
        let className = theme.type === 0 ? 'theme-skin' : 'theme-container';

        let selected = false;

        if(this.selectedAsSite()){
            className += " selected";
        }

        return className;
    }

    setSiteTheme(){
        const {props, state} = this;
        let theme = props.theme;

        //props.dispatch(ThemeActions.applyTheme(themeFile, 1));
    }

    renderActions(){
        const {props, state} = this;
        let theme = props.theme;
        let type = theme.type;

        if(this.selectedAsSite()){
            return null;
        }

        return <span className="actions">
        </span>;
    }

    renderThumbnail(){
        const {props, state} = this;

        let theme = props.theme;
        let className = 'thumbnail' + (theme.thumbnail ? '' : ' empty');

        return <span className={className}>
            {theme.thumbnail ? <img src={theme.thumbnail} /> : <EmptyThumbnail />}
            {this.renderActions()}         
        </span>;
    }
    
    render() {
        const {props, state} = this;

        return (
            <div className={this.getClassName()}>
                {this.renderThumbnail()}
                <OverflowText text={props.theme.packageName} maxWidth={168} className="title" />
            </div>
        );
    }
}

Theme.propTypes = {
    dispatch: PropTypes.func.isRequired,
    theme: PropTypes.object
};

function mapStateToProps(state) {
    return {
        currentTheme: state.theme.currentTheme
    };
}

export default connect(mapStateToProps)(Theme);