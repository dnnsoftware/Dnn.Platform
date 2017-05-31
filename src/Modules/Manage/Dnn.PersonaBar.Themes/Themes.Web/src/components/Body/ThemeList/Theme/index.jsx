import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import {
    theme as ThemeActions
} from "actions";
import Localization from "localization";
import OverflowText from "dnn-text-overflow-wrapper";
import SvgIcon from "../../SvgIcon";
import utils from "utils";
import "./style.less";

let canEdit = false;

class Theme extends Component {
    constructor() {
        super();
        this.state = {};
        canEdit = utils.params.settings.isHost || utils.params.settings.isAdmin || (utils.params.settings.permissions && utils.params.settings.permissions.EDIT === true);
    }

    selectedAsSite() {
        const {props} = this;
        let theme = props.theme;
        let currentTheme = props.currentTheme;

        if (theme.type === 0) {
            return currentTheme.SiteLayout.themeName === theme.packageName;
        } else {
            return currentTheme.SiteContainer.themeName === theme.packageName;
        }
    }

    getClassName() {
        const {props} = this;
        let theme = props.theme;
        let className = theme.type === 0 ? "theme-skin" : "theme-container";

        if (this.selectedAsSite()) {
            className += " selected";
        }

        return className;
    }

    applyDefaultTheme() {
        const {props} = this;
        let theme = props.theme;

        utils.utilities.confirm(Localization.get("ApplyConfirm"), Localization.get("Confirm"), Localization.get("Cancel"), function () {
            props.dispatch(ThemeActions.applyDefaultTheme(theme.packageName));
        });
    }

    deleteTheme() {
        const {props} = this;
        let theme = props.theme;

        utils.utilities.confirm(Localization.get("DeleteConfirm"), Localization.get("Confirm"), Localization.get("Cancel"), function () {
            props.dispatch(ThemeActions.deleteTheme(theme));
        });
    }

    previewTheme() {
        const {props} = this;
        let theme = props.theme;

        let previewUrl = utils.params.settings.previewUrl;
        window.open(previewUrl + "?SkinSrc=" + theme.defaultThemeFile);
    }

    renderActions() {
        const {props} = this;
        let theme = props.theme;

        if (this.selectedAsSite()) {
            return <span className="checkmark"><SvgIcon name="Checkmark" /></span>;
        }

        let isHost = utils.params.settings.isHost;
        return <span className="actions">
            <ul className={(isHost || theme.level === 1) ? "" : "short"}>
                <li onClick={this.previewTheme.bind(this) } title={Localization.get("PreviewTheme") }><SvgIcon name="View" /></li>
                {canEdit && <li onClick={this.applyDefaultTheme.bind(this) } title={Localization.get("ApplyTheme") }><SvgIcon name="Apply" /></li>}
                {((isHost || theme.level === 1) && theme.canDelete) && <li onClick={this.deleteTheme.bind(this) } title={Localization.get("DeleteTheme") }><SvgIcon name="Trash" /></li>}
            </ul>
        </span>;
    }

    renderThumbnail() {
        const {props} = this;

        let theme = props.theme;
        let className = "thumbnail" + (theme.thumbnail ? "" : " empty");

        return <span className={className}>
            {theme.thumbnail ? <img src={theme.thumbnail} alt={theme.packageName} /> : <SvgIcon name="EmptyThumbnail" />}
            {this.renderActions() }
        </span>;
    }

    render() {
        const {props} = this;

        return (
            <div className={this.getClassName() }>
                {this.renderThumbnail() }
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