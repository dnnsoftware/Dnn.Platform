import React, {Component, PropTypes} from "react";
import { Scrollbars } from "react-custom-scrollbars";
import GridCell from "dnn-grid-cell";
import style from "./style.less";

class Gallery extends Component {

    render() {        
        return (
            <div className={style.moduleContainer}>
                <Scrollbars
                    className="container"
                    autoHeight
                    autoHeightMin={0}
                    autoHeightMax={480}>
                    <GridCell>
                        {this.props.children}
                    </GridCell>
                </Scrollbars>
            </div>
        );
    }
}

Gallery.propTypes = {
    children: PropTypes.node
};

export default Gallery;