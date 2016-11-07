import React, {Component, PropTypes} from "react";
import styles from "./style.less";
import Switch from "dnn-switch";
import localization from "../../../../localization";
import Label from "dnn-label";


class PageUrlCommons extends Component {

    onChangeField(key) {
        const {onChangeField} = this.props;
        onChangeField(key, event.target.value);
    }

    render() {
        const {page, display} = this.props;
        const gridClass = "page-url-commons-grid " + display; 
        
        return (
            <div className={styles.pageUrlCommons}>
                <div className={gridClass}>
                    <div className="left-column">
                        <Label
                            labelType="inline"
                            tooltipMessage={localization.get("permanent_redirect_tooltip")}
                            label={localization.get("Permanent Redirect")}
                            />
                        <Switch
                            labelHidden={true}
                            value={page.permanentRedirect}
                            onChange={this.onChangeField.bind(this, "permanentRedirect")} />
                    </div>
                    <div className="right-column">
                        <Label
                            labelType="inline"
                            tooltipMessage={localization.get("open_new_window_tooltip")}
                            label={localization.get("Open Link in New Window")}
                            />
                        <Switch
                            labelHidden={true}
                            value={page.openNewWindow}
                            onChange={this.onChangeField.bind(this, "openNewWindow")} />
                    </div>
                </div>
            </div>
        );
    }
}

PageUrlCommons.propTypes = {
    page: PropTypes.object.isRequired,
    onChangeField: PropTypes.func.isRequired,
    display: PropTypes.oneOf(["horizontal", "vertical"]).isRequired
};

PageUrlCommons.defaultProps = {
    display: "horizontal" 
};

export default PageUrlCommons;