import React, {PropTypes} from "react";
import Localization from "../../../localization";
import styles from "./style.less";

const BackToMain = ({onClick}) => {
    return (
        <div 
            className={styles.backToMain} 
            onClick={onClick}>
            {Localization.get("BackToPages")}
        </div>
    );
};

BackToMain.propTypes = {
    onClick: PropTypes.func
};

export default BackToMain;