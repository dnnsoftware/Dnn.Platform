import React, {Component, PropTypes} from "react";
import localization from "../../../localization";
import styles from "./style.less";
import GridSystem from "dnn-grid-system";
import GridCell from "dnn-grid-cell";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import Switch from "dnn-switch";
import Label from "dnn-label";

class PageDetailsFooter extends Component {

    onChangeField(key, event) {
        const {onChangeField} = this.props;
        onChangeField(key, event.target.value);
    }

    render() {
        const {page} = this.props;
        const normalPage = page.pageType === "normal";

        return (
            <div className={styles.pageStandard}>
                {!normalPage &&
                    <GridCell className="left-column">
                        <SingleLineInputWithError
                            label={localization.get("Name")}
                            tooltipMessage={localization.get("page_name_tooltip")}
                            value={page.name} 
                            onChange={this.onChangeField.bind(this, "name")} />
                    </GridCell>
                }
                <GridSystem>
                    <GridCell className="left-column">
                        <Label
                            labelType="inline"
                            tooltipMessage={localization.get("display_in_menu_tooltip")}
                            label={localization.get("Display In Menu")}
                            />
                        <Switch
                            labelHidden={true}
                            value={page.includeInMenu}
                            onChange={this.onChangeField.bind(this, "displayInMenu")} />
                    </GridCell>
                    <GridCell className="right-column">
                        <Label
                            labelType="inline"
                            tooltipMessage={localization.get("enable_scheduling_tooltip")}
                            label={localization.get("Enable Scheduling")}
                            />
                        <Switch
                            labelHidden={true}
                            value={page.enableScheduling}
                            onChange={this.onChangeField.bind(this, "enableScheduling")} />
                    </GridCell>
                </GridSystem>
            </div>
        );
    }
}

PageDetailsFooter.propTypes = {
    page: PropTypes.object.isRequired,
    onChangeField: PropTypes.func.isRequired
};

export default PageDetailsFooter;