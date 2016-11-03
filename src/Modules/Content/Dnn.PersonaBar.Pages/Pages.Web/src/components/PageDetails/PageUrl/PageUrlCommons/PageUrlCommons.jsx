import React, {Component, PropTypes} from "react";
import styles from "./style.less";
import GridSystem from "dnn-grid-system";
import GridCell from "dnn-grid-cell";
import Switch from "dnn-switch";
import localization from "../../../../localization";
import Label from "dnn-label";


class PageUrlCommons extends Component {

    onChangeField(key, value){
        const {onChangeField} = this.props;
        onChangeField(key, event.target.value);
    }

    render() {
        const {page} = this.props;
        
        return (
            <div className={styles.pageUrlCommons}>
                <GridSystem className="page-url-commons-grid">
                    <GridCell className="left-column">
                        <Label
                            labelType="inline"
                            tooltipMessage={localization.get("permanent_redirect_tooltip")}
                            label={localization.get("Permanent Redirect")}
                            />
                        <Switch
                            labelHidden={true}
                            value={page.permanentRedirect}
                            onChange={this.onChangeField.bind(this, "permanentRedirect")} />
                    </GridCell>
                    <GridCell className="right-column">
                        <Label
                            labelType="inline"
                            tooltipMessage={localization.get("open_new_window_tooltip")}
                            label={localization.get("Open Link in New Window")}
                            />
                        <Switch
                            labelHidden={true}
                            value={page.openNewWindow}
                            onChange={this.onChangeField.bind(this, "openNewWindow")} />
                    </GridCell>
                </GridSystem>
                <div style={{clear: "both"}}></div>
            </div>
        );
    }
}

PageUrlCommons.propTypes = {
    page: PropTypes.object.isRequired,
    onChangeField: PropTypes.func.isRequired
};

export default PageUrlCommons;