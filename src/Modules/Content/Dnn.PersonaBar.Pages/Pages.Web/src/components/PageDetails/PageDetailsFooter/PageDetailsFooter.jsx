import React, {Component, PropTypes} from "react";
import localization from "../../../localization";
import styles from "./style.less";
import GridSystem from "dnn-grid-system";
import GridCell from "dnn-grid-cell";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import Switch from "dnn-switch";
import Label from "dnn-label";
import DatePicker from "dnn-date-picker";

class PageDetailsFooter extends Component {

    onChangeField(key, event) {
        const {onChangeField} = this.props;
        onChangeField(key, event.target.value);
    }

    onChangeValue(key, value) {
        const {onChangeField} = this.props;
        onChangeField(key, value);
    }

    render() {
        const {page} = this.props;
        const normalPage = page.pageType === "normal";

        return (
            <div className={styles.pageDetailsFooter}>
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
                            onChange={this.onChangeValue.bind(this, "includeInMenu")} />
                    </GridCell>
                    <GridCell className="right-column">
                        <Label
                            labelType="inline"
                            tooltipMessage={localization.get("enable_scheduling_tooltip")}
                            label={localization.get("Enable Scheduling")}
                            />
                        <Switch
                            labelHidden={true}
                            value={page.schedulingEnabled}
                            onChange={this.onChangeValue.bind(this, "schedulingEnabled")} />
                        <div style={{clear: "both"}}></div>
                        {page.schedulingEnabled &&
                            <div className="scheduler-date-box">
                                <div className="scheduler-date-row">
                                    <Label
                                        label={localization.get("Start date")} />
                                    <DatePicker
                                        date={page.startDate}
                                        updateDate={this.onChangeValue.bind(this, "startDate")}
                                        isDateRange={false}
                                        hasTimePicker={true}
                                        showClearDateButton={false} />
                                </div>
                                <div className="scheduler-date-row">
                                    <Label
                                        label={localization.get("End date")} />
                                    <DatePicker
                                        date={page.endDate}
                                        updateDate={this.onChangeValue.bind(this, "endDate")}
                                        isDateRange={false}
                                        hasTimePicker={true}
                                        showClearDateButton={false} />
                                </div>
                            </div>
                        }
                    </GridCell>
                </GridSystem>
                <div style={{clear: "both"}}></div>
            </div>
        );
    }
}

PageDetailsFooter.propTypes = {
    page: PropTypes.object.isRequired,
    onChangeField: PropTypes.func.isRequired
};

export default PageDetailsFooter;