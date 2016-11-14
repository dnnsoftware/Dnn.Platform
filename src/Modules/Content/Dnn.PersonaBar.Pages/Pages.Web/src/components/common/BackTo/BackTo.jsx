import React, {PropTypes} from "react";
import styles from "./style.less";

const BackToMain = ({onClick, label}) => {
    return (
        <div 
            className={styles.backTo} 
            onClick={onClick}>
            {label}
        </div>
    );
};

BackToMain.propTypes = {
    label: PropTypes.string.isRequired,
    onClick: PropTypes.func
};

export default BackToMain;