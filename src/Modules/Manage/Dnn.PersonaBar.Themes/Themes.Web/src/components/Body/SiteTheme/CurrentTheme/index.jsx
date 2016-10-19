import React, {Component, PropTypes } from "react";
import { connect } from "react-redux";
import {
    theme as ThemeActions
} from "actions";
import Localization from "localization";
import GridCell from "dnn-grid-cell";
import Button from "dnn-button";

import SvgIcon from "../../SvgIcon";

import "./style.less";

class CurrentTheme extends Component {
    constructor() {
        super();
        this.state = {};
    }

    
    render() {
        const {props, state} = this;

        return (
            <div className="current-theme">
            {
                (function(){
                    if(props.theme.SiteLayout.thumbnail){
                        return <img src={props.theme.SiteLayout.thumbnail} />;
                    }
                    else{
                        return <SvgIcon name="EmptyThumbnail" />;
                    }
                })()
            }
            </div>
        );
    }
}

CurrentTheme.propTypes = {
    dispatch: PropTypes.func.isRequired,
    theme: PropTypes.object
};

function mapStateToProps(state) {
    return {
    };
}

export default connect(mapStateToProps)(CurrentTheme);